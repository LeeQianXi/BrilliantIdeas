using System.Diagnostics.Contracts;
using NetUtility.Cache;

namespace NetUtility;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static partial class Extension
{
    private const BindingFlags DeclaredFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                               BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static readonly Type[] EmptyTypes = [];

    extension(Type type)
    {
        [Pure]
        public MemberInfo[] GetDeclaredMembers()
        {
            return type.GetMembers(DeclaredFlags);
        }

        [Pure]
        public FieldInfo[] GetDeclaredFields()
        {
            return type.GetFields(DeclaredFlags);
        }

        [Pure]
        public PropertyInfo[] GetDeclaredProperties()
        {
            return type.GetProperties(DeclaredFlags);
        }

        [Pure]
        public MethodInfo[] GetDeclaredMethods()
        {
            return type.GetMethods(DeclaredFlags);
        }

        [Pure]
        public ConstructorInfo[] GetDeclaredConstructors()
        {
            return type.GetConstructors(DeclaredFlags & ~BindingFlags.Static); // LUDIQ: Exclude static constructors
        }

        [Pure]
        public MemberInfo? GetDeclaredMember(string memberName)
        {
            var members = GetDeclaredMembers(type);
            return members.FirstOrDefault(t => t.Name == memberName);
        }

        [Pure]
        public MemberInfo? GetDeclaredField(string fieldName)
        {
            var fields = GetDeclaredFields(type);
            return fields.FirstOrDefault(t => t.Name == fieldName);
        }

        [Pure]
        public PropertyInfo? GetDeclaredProperty(string propertyName)
        {
            var props = GetDeclaredProperties(type);
            return props.FirstOrDefault(t => t.Name == propertyName);
        }

        [Pure]
        public MethodInfo? GetDeclaredMethod(string methodName)
        {
            var methods = GetDeclaredMethods(type);
            return methods.FirstOrDefault(t => t.Name == methodName);
        }

        [Pure]
        public ConstructorInfo? GetDeclaredConstructor(Type[] parameters)
        {
            foreach (var ctor in GetDeclaredConstructors(type))
                if (ExactMatch(ctor))
                    return ctor;
            return null;

            bool ExactMatch(ConstructorInfo ctor)
            {
                var @params = ctor.GetParameters();
                if (@params.Length != parameters.Length)
                    return false;
                return !@params.Where((t, i) => t.ParameterType != parameters[i]).Any();
            }
        }
    }

    extension(Type type)
    {
        [Pure]
        public MemberInfo[] GetFlattenedMember(string memberName)
        {
            var result = new List<MemberInfo>();
            var cur = type;
            while (cur != null)
            {
                var members = GetDeclaredMembers(cur);

                result.AddRange(members.Where(t => t.Name == memberName));

                cur = cur.Resolve().BaseType;
            }

            return result.ToArray();
        }

        [Pure]
        public IEnumerable<MethodInfo> GetFlattenedMethods(string methodName)
        {
            var cur = type;
            while (cur != null)
            {
                var methods = GetDeclaredMethods(cur);

                foreach (var t in methods)
                    if (t.Name == methodName)
                        yield return t;

                cur = cur.Resolve().BaseType;
            }
        }

        [Pure]
        public MethodInfo? GetFlattenedMethod(string methodName)
        {
            var cur = type;
            while (cur != null)
            {
                var methods = GetDeclaredMethods(cur);
                foreach (var t in methods)
                    if (t.Name == methodName)
                        return t;
                cur = cur.Resolve().BaseType;
            }

            return null;
        }

        [Pure]
        public PropertyInfo? GetFlattenedProperty(string propertyName)
        {
            var cur = type;
            while (cur != null)
            {
                var properties = GetDeclaredProperties(cur);
                foreach (var t in properties)
                    if (t.Name == propertyName)
                        return t;
                cur = cur.Resolve().BaseType;
            }

            return null;
        }

        [Pure]
        public MemberInfo AsMemberInfo()
        {
            return type;
        }

        [Pure]
        public Type Resolve()
        {
            return type;
        }
    }

    extension(MemberInfo member)
    {
        [Pure]
        public bool IsType()
        {
            return member is Type;
        }

        [Pure]
        public Type AsType()
        {
            return (Type)member;
        }
    }


    #region Attribute Queries

    private record AttributeQuery
    {
        public required MemberInfo MemberInfo { get; init; }
        public required Type AttributeType { get; init; }

        public virtual bool Equals(AttributeQuery? other)
        {
            return other is not null && MemberInfo == other.MemberInfo && AttributeType == other.AttributeType;
        }

        public override int GetHashCode()
        {
            return
                MemberInfo.GetHashCode() +
                17 * AttributeType.GetHashCode();
        }
    }

    private static readonly IReferenceCache<AttributeQuery, Attribute> CachedAttributeQueries =
        new LruCache<AttributeQuery, Attribute>();

    extension(MemberInfo element)
    {
        /// <summary>
        ///     Returns true if the given attribute is defined on the given element.
        /// </summary>
        [Pure]
        public bool HasAttribute<TAttribute>()
        {
            return HasAttribute(element, typeof(TAttribute));
        }

        /// <summary>
        ///     Returns true if the given attribute is defined on the given element.
        /// </summary>
        [Pure]
        public bool HasAttribute<TAttribute>(bool shouldCache)
        {
            return HasAttribute(element, typeof(TAttribute), shouldCache);
        }

        /// <summary>
        ///     Returns true if the given attribute is defined on the given element.
        /// </summary>
        [Pure]
        public bool HasAttribute(Type attributeType)
        {
            return HasAttribute(element, attributeType, true);
        }

        /// <summary>
        ///     Returns true if the given attribute is defined on the given element.
        /// </summary>
        [Pure]
        public bool HasAttribute(Type attributeType, bool shouldCache)
        {
            // LAZLO / LUDIQ FIX
            // Inheritance on property overrides. MemberInfo.IsDefined ignores the inherited parameter.
            // https://stackoverflow.com/questions/2520035
            return Attribute.IsDefined(element, attributeType, true);
            //return element.IsDefined(attributeType, true);
        }

        /// <summary>
        ///     Fetches the given attribute from the given MemberInfo. This method
        ///     applies caching and is allocation free (after caching has been
        ///     performed).
        /// </summary>
        /// <param name="attributeType">The type of attribute to fetch.</param>
        /// <param name="shouldCache"></param>
        /// <returns>The attribute or null.</returns>
        [Pure]
        public Attribute? GetAttribute(Type attributeType, bool shouldCache)
        {
            var query = new AttributeQuery
            {
                MemberInfo = element,
                AttributeType = attributeType
            };
            if (CachedAttributeQueries.TryGetValue(query, out var attribute)) return attribute;
            // LAZLO / LUDIQ FIX
            // Inheritance on property overrides. MemberInfo.IsDefined ignores the inherited parameter
            //var attributes = element.GetCustomAttributes(attributeType, /*inherit:*/ true).ToArray();
            var attributes = Attribute.GetCustomAttributes(element, attributeType, true);
            attribute = attributes.FirstOrDefault();
            if (attribute is null) return null;
            if (shouldCache) CachedAttributeQueries.Update(query, attribute);
            return attribute;
        }

        /// <summary>
        ///     Fetches the given attribute from the given MemberInfo.
        /// </summary>
        /// <typeparam name="TAttribute">
        ///     The type of attribute to fetch.
        /// </typeparam>
        /// <param name="shouldCache">
        ///     Should this computation be cached? If this is the only time it will
        ///     ever be done, don't bother caching.
        /// </param>
        /// <returns>The attribute or null.</returns>
        [Pure]
        public TAttribute? GetAttribute<TAttribute>(bool shouldCache)
            where TAttribute : Attribute
        {
            return (TAttribute?)GetAttribute(element, typeof(TAttribute), shouldCache);
        }

        [Pure]
        public TAttribute? GetAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            return GetAttribute<TAttribute>(element, true);
        }
    }

    #endregion Attribute Queries
}