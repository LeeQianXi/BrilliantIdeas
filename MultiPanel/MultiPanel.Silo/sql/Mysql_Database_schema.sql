-- MySQL_Database_Schema.sql

-- 创建数据库
CREATE
    DATABASE IF NOT EXISTS OrleansAuthentication
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE
    OrleansAuthentication;

-- 1. 用户表
CREATE TABLE IF NOT EXISTS UserAccounts
(
    UserId
                        VARCHAR(150) NOT NULL,
    Username            VARCHAR(100) NOT NULL,
    Email               VARCHAR(255) NOT NULL,
    DisplayName         VARCHAR(100) NULL,
    PasswordHash        LONGTEXT     NOT NULL,
    Roles               JSON         NULL,
    Claims              JSON         NULL,
    CreatedAt           DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                      (
            6
                                                      ),
    UpdatedAt           DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                      (
            6
                                                      ) ON UPDATE CURRENT_TIMESTAMP
                                                                  (
            6
                                                                  ),
    LastLogin           DATETIME(6)  NULL,
    IsActive            BOOLEAN      NOT NULL DEFAULT TRUE,
    EmailVerified       BOOLEAN      NOT NULL DEFAULT FALSE,
    FailedLoginAttempts INT          NOT NULL DEFAULT 0,
    LockoutEnd          DATETIME(6)  NULL,
    ActiveSessions      JSON         NULL,
    PRIMARY KEY
        (
         UserId
            ),
    UNIQUE KEY IX_UserAccounts_Username
        (
         Username
            ),
    UNIQUE KEY IX_UserAccounts_Email
        (
         Email
            ),
    INDEX IX_UserAccounts_IsActive
        (
         IsActive
            ),
    INDEX IX_UserAccounts_LastLogin
        (
         LastLogin
            )
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- 2. 会话表
CREATE TABLE IF NOT EXISTS Sessions
(
    SessionId
                          CHAR(36)     NOT NULL,
    UserId                VARCHAR(150) NOT NULL,
    Username              VARCHAR(100) NOT NULL,
    CreatedAt             DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                        (
            6
                                                        ),
    ExpiresAt             DATETIME(6)  NOT NULL,
    RefreshTokenExpiresAt DATETIME(6)  NOT NULL,
    IpAddress             VARCHAR(50)  NULL,
    UserAgent             VARCHAR(500) NULL,
    IsActive              BOOLEAN      NOT NULL DEFAULT TRUE,
    Claims                JSON         NULL,
    AccessTokenHash       LONGTEXT     NOT NULL,
    RefreshTokenHash      LONGTEXT     NOT NULL,
    RevokedAt             DATETIME(6)  NULL,
    RevokedReason         VARCHAR(255) NULL,
    PRIMARY KEY
        (
         SessionId
            ),
    INDEX IX_Sessions_UserId
        (
         UserId
            ),
    INDEX IX_Sessions_ExpiresAt
        (
         ExpiresAt
            ),
    INDEX IX_Sessions_IsActive
        (
         IsActive
            ),
    INDEX IX_Sessions_User_Active
        (
         UserId,
         IsActive,
         ExpiresAt
            ),
    CONSTRAINT FK_Sessions_UserAccounts FOREIGN KEY
        (
         UserId
            )
        REFERENCES UserAccounts
            (
             UserId
                ) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- 3. 令牌表
CREATE TABLE IF NOT EXISTS Tokens
(
    TokenId
              VARCHAR(150) NOT NULL,
    UserId    VARCHAR(150) NOT NULL,
    SessionId CHAR(36)     NOT NULL,
    TokenHash LONGTEXT     NOT NULL,
    CreatedAt DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP
                                            (
            6
                                            ),
    ExpiresAt DATETIME(6)  NOT NULL,
    IsRevoked BOOLEAN      NOT NULL DEFAULT FALSE,
    RevokedAt DATETIME(6)  NULL,
    TokenType VARCHAR(50)  NOT NULL,
    Scopes    JSON         NULL,
    IsUsed    BOOLEAN      NOT NULL DEFAULT FALSE,
    PRIMARY KEY
        (
         TokenId
            ),
    INDEX IX_Tokens_UserId
        (
         UserId
            ),
    INDEX IX_Tokens_SessionId
        (
         SessionId
            ),
    INDEX IX_Tokens_ExpiresAt
        (
         ExpiresAt
            ),
    INDEX IX_Tokens_IsRevoked
        (
         IsRevoked
            ),
    INDEX IX_Tokens_Expired
        (
         ExpiresAt,
         IsRevoked,
         IsUsed
            ),
    CONSTRAINT FK_Tokens_Sessions FOREIGN KEY
        (
         SessionId
            )
        REFERENCES Sessions
            (
             SessionId
                ) ON DELETE CASCADE,
    CONSTRAINT FK_Tokens_UserAccounts FOREIGN KEY
        (
         UserId
            )
        REFERENCES UserAccounts
            (
             UserId
                )
        ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- 5. 存储过程：清理过期数据
DELIMITER
$$

CREATE PROCEDURE IF NOT EXISTS CleanupExpiredData(
    IN daysToKeep INT
)
BEGIN
    DECLARE
        deletedRows INT DEFAULT 0;

    -- 清理过期会话（保留指定天数）
    DELETE
    FROM Sessions
    WHERE ExpiresAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL daysToKeep DAY);

    SET
        deletedRows = deletedRows + ROW_COUNT();

    -- 清理过期令牌
    DELETE
    FROM Tokens
    WHERE ExpiresAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL daysToKeep DAY);

    SET
        deletedRows = deletedRows + ROW_COUNT();

    -- 更新过期的活跃会话状态
    UPDATE Sessions
    SET IsActive = FALSE
    WHERE IsActive = TRUE
      AND ExpiresAt < UTC_TIMESTAMP();

    SET
        deletedRows = deletedRows + ROW_COUNT();

    SELECT deletedRows AS TotalRowsAffected;
END$$

DELIMITER ;

-- 6. 存储过程：获取用户统计信息
DELIMITER
$$

CREATE PROCEDURE IF NOT EXISTS GetUserStatistics(
    IN userId VARCHAR(150)
)
BEGIN
    IF
        userId IS NULL THEN
        -- 获取所有用户统计
        SELECT COUNT(*)                 AS TotalUsers,
               SUM(IsActive)            AS ActiveUsers,
               SUM(EmailVerified)       AS VerifiedUsers,
               AVG(FailedLoginAttempts) AS AvgFailedAttempts,
               MAX(UpdatedAt)           AS LastUpdated
        FROM UserAccounts;
    ELSE
        -- 获取特定用户统计
        SELECT u.UserId,
               u.Username,
               u.Email,
               u.DisplayName,
               u.CreatedAt,
               u.LastLogin,
               u.IsActive,
               u.EmailVerified,
               u.FailedLoginAttempts,
               COUNT(s.SessionId) AS ActiveSessionsCount,
               MAX(s.ExpiresAt)   AS LatestSessionExpiry
        FROM UserAccounts u
                 LEFT JOIN Sessions s ON u.UserId = s.UserId AND s.IsActive = TRUE
        WHERE u.UserId = userId
        GROUP BY u.UserId, u.Username, u.Email, u.DisplayName,
                 u.CreatedAt, u.LastLogin, u.IsActive,
                 u.EmailVerified, u.FailedLoginAttempts;
    END IF;
END$$

DELIMITER ;

-- 7. 创建视图：活跃用户会话
CREATE OR REPLACE VIEW ActiveUserSessions AS
SELECT u.UserId,
       u.Username,
       u.Email,
       u.DisplayName,
       s.SessionId,
       s.CreatedAt                                         AS SessionCreated,
       s.ExpiresAt                                         AS SessionExpires,
       s.IpAddress,
       s.UserAgent,
       TIMESTAMPDIFF(MINUTE, UTC_TIMESTAMP(), s.ExpiresAt) AS MinutesRemaining
FROM UserAccounts u
         INNER JOIN Sessions s ON u.UserId = s.UserId
WHERE s.IsActive = TRUE
  AND s.ExpiresAt > UTC_TIMESTAMP()
  AND u.IsActive = TRUE;

-- 8. 创建事件：定期清理
SET
    GLOBAL event_scheduler = ON;

DELIMITER
$$

CREATE
    EVENT IF NOT EXISTS DailyCleanup
    ON SCHEDULE EVERY 1 DAY
        STARTS CURRENT_TIMESTAMP
    DO
    BEGIN
        -- 清理7天前的过期数据
        CALL CleanupExpiredData(7);
    END$$

DELIMITER ;

-- 9. 创建函数：生成安全令牌
DELIMITER
$$

CREATE FUNCTION IF NOT EXISTS GenerateSecureToken(length INT)
    RETURNS VARCHAR(255)
    DETERMINISTIC
BEGIN
    DECLARE
        chars VARCHAR(62) DEFAULT 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    DECLARE
        result VARCHAR(255) DEFAULT '';
    DECLARE
        i INT DEFAULT 0;

    WHILE
        i < length
        DO
            SET result = CONCAT(result, SUBSTRING(chars, FLOOR(1 + RAND() * 62), 1));
            SET
                i = i + 1;
        END WHILE;

    RETURN result;
END$$

DELIMITER ;

-- 10. 创建审计表（可选）
CREATE TABLE IF NOT EXISTS AuditLogs
(
    AuditId
               BIGINT
        AUTO_INCREMENT,
    Action
               VARCHAR(50)  NOT NULL,
    UserId     VARCHAR(150) NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId   VARCHAR(150) NULL,
    OldValues  JSON         NULL,
    NewValues  JSON         NULL,
    IpAddress  VARCHAR(50)  NULL,
    UserAgent  VARCHAR(500) NULL,
    CreatedAt  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP
                                             (
            6
                                             ),
    CreatedBy  VARCHAR(150) NULL,
    PRIMARY KEY
        (
         AuditId
            ),
    INDEX IX_AuditLogs_UserId
        (
         UserId
            ),
    INDEX IX_AuditLogs_CreatedAt
        (
         CreatedAt
            ),
    INDEX IX_AuditLogs_Action
        (
         Action
            )
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- 11. 创建触发器：用户更新审计
DELIMITER
$$

CREATE TRIGGER IF NOT EXISTS trg_UserAccounts_AfterUpdate
    AFTER
        UPDATE
    ON UserAccounts
    FOR EACH ROW
BEGIN
    INSERT INTO AuditLogs (Action,
                           UserId,
                           EntityType,
                           EntityId,
                           OldValues,
                           NewValues,
                           CreatedBy)
    VALUES ('UPDATE',
            NEW.UserId,
            'UserAccount',
            NEW.UserId,
            JSON_OBJECT(
                    'Username', OLD.Username,
                    'Email', OLD.Email,
                    'DisplayName', OLD.DisplayName,
                    'IsActive', OLD.IsActive,
                    'FailedLoginAttempts', OLD.FailedLoginAttempts,
                    'UpdatedAt', OLD.UpdatedAt
            ),
            JSON_OBJECT(
                    'Username', NEW.Username,
                    'Email', NEW.Email,
                    'DisplayName', NEW.DisplayName,
                    'IsActive', NEW.IsActive,
                    'FailedLoginAttempts', NEW.FailedLoginAttempts,
                    'UpdatedAt', NEW.UpdatedAt
            ),
            NEW.UserId);
END$$

DELIMITER ;

-- 12. 创建备份表（用于软删除）
CREATE TABLE IF NOT EXISTS UserAccounts_Archive LIKE UserAccounts;
ALTER TABLE UserAccounts_Archive
    ADD COLUMN ArchivedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);
ALTER TABLE UserAccounts_Archive
    ADD COLUMN ArchivedBy VARCHAR(150) NULL;