-- 餐桌表（用于餐桌管理模块）
CREATE TABLE IF NOT EXISTS `dining_table` (
    `dining_table_id`  BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '自增主键',
    `table_no`         VARCHAR(50) NOT NULL COMMENT '餐桌号（业务唯一标识）',
    `seat_count`       INT NOT NULL DEFAULT 4 COMMENT '容纳人数',
    `table_status`     INT NOT NULL DEFAULT 1 COMMENT '状态：1=空闲，2=停用，3=使用中',
    `qr_code_image_url` VARCHAR(500) DEFAULT '' COMMENT '二维码图片URL',
    `created_at`       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    UNIQUE KEY `uk_table_no` (`table_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='餐桌表';
