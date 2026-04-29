/*
 Navicat Premium Data Transfer

 Source Server         : MySql1
 Source Server Type    : MySQL
 Source Server Version : 80036 (8.0.36)
 Source Host           : localhost:3306
 Source Schema         : nengjidb_v1

 Target Server Type    : MySQL
 Target Server Version : 80036 (8.0.36)
 File Encoding         : 65001

 Date: 27/04/2026 21:25:31
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for activity
-- ----------------------------
DROP TABLE IF EXISTS `activity`;
CREATE TABLE `activity`  (
  `activity_id` bigint NOT NULL AUTO_INCREMENT COMMENT '活动ID',
  `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '活动标题',
  `price` decimal(10, 2) NOT NULL COMMENT '价格',
  `start_date` datetime NOT NULL COMMENT '开始日期',
  `end_date` datetime NOT NULL COMMENT '结束日期',
  `image_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '活动图片',
  `description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '活动简介',
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '活动地点',
  `people` int NOT NULL DEFAULT 0 COMMENT '活动报名上限人数',
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '活动内容',
  `status_id` int NOT NULL DEFAULT 1 COMMENT '活动状态ID',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `type_id` int NOT NULL COMMENT '活动类型',
  PRIMARY KEY (`activity_id`) USING BTREE,
  INDEX `idx_activity_sort`(`sort_order` ASC) USING BTREE,
  INDEX `idx_activity_status_id`(`status_id` ASC) USING BTREE,
  INDEX `idx_activity_date`(`start_date` ASC, `end_date` ASC) USING BTREE,
  INDEX `fk_activity_activity_type_2`(`type_id` ASC) USING BTREE,
  CONSTRAINT `fk_activity_activity_type_2` FOREIGN KEY (`type_id`) REFERENCES `activity_type` (`activity_type_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_activity_status` FOREIGN KEY (`status_id`) REFERENCES `activity_status` (`activity_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity
-- ----------------------------
INSERT INTO `activity` VALUES (1, '周末草莓采摘体验', 39.90, '2026-05-01 09:00:00', '2026-05-01 12:00:00', '/images/farm/Farm_10.jpg', '周末到农场体验草莓采摘。', '能记农场草莓园', 50, '包含采摘讲解、入园体验和草莓品尝。', 1, 1, '2026-04-27 11:10:00', 1);
INSERT INTO `activity` VALUES (2, '亲子农场研学课', 59.90, '2026-05-02 14:00:00', '2026-05-02 17:00:00', '/images/farm/Farm_29.jpg', '适合亲子家庭参加的农场研学活动。', '能记农场研学区', 40, '包含作物认知、农具体验和亲子互动课程。', 1, 2, '2026-04-27 11:11:00', 2);

-- ----------------------------
-- Table structure for activity_material
-- ----------------------------
DROP TABLE IF EXISTS `activity_material`;
CREATE TABLE `activity_material`  (
  `activity_material_id` bigint NOT NULL AUTO_INCREMENT COMMENT '素材ID',
  `activity_id` bigint NOT NULL COMMENT '活动ID',
  `material_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '素材类型 0图 1详情图 2视频',
  `material_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '素材地址',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序',
  PRIMARY KEY (`activity_material_id`) USING BTREE,
  INDEX `idx_activity_material_activity`(`activity_id` ASC, `sort_order` ASC) USING BTREE,
  CONSTRAINT `fk_activity_material_activity` FOREIGN KEY (`activity_id`) REFERENCES `activity` (`activity_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动素材表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_material
-- ----------------------------
INSERT INTO `activity_material` VALUES (1, 1, 'image', '/images/farm/Farm_10.jpg', '2026-04-27 11:20:00', 1);
INSERT INTO `activity_material` VALUES (2, 1, 'video', '/videos/farm_intro.mp4', '2026-04-27 11:21:00', 2);
INSERT INTO `activity_material` VALUES (3, 2, 'image', '/images/farm/Farm_29.jpg', '2026-04-27 11:22:00', 1);

-- ----------------------------
-- Table structure for activity_order_detail
-- ----------------------------
DROP TABLE IF EXISTS `activity_order_detail`;
CREATE TABLE `activity_order_detail`  (
  `activity_order_details_id` bigint NOT NULL AUTO_INCREMENT COMMENT '活动订单明细ID',
  `activity_order_id` bigint NOT NULL COMMENT '活动订单ID',
  `activity_id` bigint NOT NULL COMMENT '活动ID',
  `unit_price` decimal(10, 2) NOT NULL COMMENT '单价',
  `quantity` int NOT NULL COMMENT '参与人数',
  `subtotal_amount` decimal(10, 2) NOT NULL COMMENT '小计金额',
  `activity_qrcode` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '核销码URL',
  PRIMARY KEY (`activity_order_details_id`) USING BTREE,
  INDEX `idx_activity_order_detail_order_id`(`activity_order_id` ASC) USING BTREE,
  INDEX `idx_activity_order_detail_activity_id`(`activity_id` ASC) USING BTREE,
  CONSTRAINT `fk_activity_order_detail_activity` FOREIGN KEY (`activity_id`) REFERENCES `activity` (`activity_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_activity_order_detail_order` FOREIGN KEY (`activity_order_id`) REFERENCES `activity_orders` (`order_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动订单明细表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_order_detail
-- ----------------------------
INSERT INTO `activity_order_detail` VALUES (1, 1, 1, 39.90, 1, 39.90, '/images/qrcode/activity_001.png');
INSERT INTO `activity_order_detail` VALUES (2, 2, 2, 59.90, 2, 119.80, '/images/qrcode/activity_002.png');

-- ----------------------------
-- Table structure for activity_order_status
-- ----------------------------
DROP TABLE IF EXISTS `activity_order_status`;
CREATE TABLE `activity_order_status`  (
  `activity_order_status_id` int NOT NULL AUTO_INCREMENT COMMENT '活动订单状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`activity_order_status_id`) USING BTREE,
  UNIQUE INDEX `uk_activity_order_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动订单状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_order_status
-- ----------------------------
INSERT INTO `activity_order_status` VALUES (4, '已取消');
INSERT INTO `activity_order_status` VALUES (3, '已核销');
INSERT INTO `activity_order_status` VALUES (6, '已退款');
INSERT INTO `activity_order_status` VALUES (1, '待付款');
INSERT INTO `activity_order_status` VALUES (2, '待核销');
INSERT INTO `activity_order_status` VALUES (5, '退款中');

-- ----------------------------
-- Table structure for activity_orders
-- ----------------------------
DROP TABLE IF EXISTS `activity_orders`;
CREATE TABLE `activity_orders`  (
  `order_id` bigint NOT NULL AUTO_INCREMENT COMMENT '活动订单ID',
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '平台订单号',
  `wx_pay_no` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信支付订单号',
  `total_amount` decimal(10, 2) NOT NULL COMMENT '订单总金额',
  `total_quantity` int NOT NULL COMMENT '订单总人数',
  `order_status_id` int NOT NULL COMMENT '订单状态ID',
  `user_id` int NOT NULL COMMENT '下单人ID',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '下单时间',
  PRIMARY KEY (`order_id`) USING BTREE,
  UNIQUE INDEX `uk_activity_orders_order_no`(`order_no` ASC) USING BTREE,
  INDEX `idx_activity_orders_status_id`(`order_status_id` ASC) USING BTREE,
  INDEX `idx_activity_orders_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_activity_orders_create_time`(`create_time` ASC) USING BTREE,
  CONSTRAINT `fk_activity_orders_status` FOREIGN KEY (`order_status_id`) REFERENCES `activity_order_status` (`activity_order_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_activity_orders_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动订单表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_orders
-- ----------------------------
INSERT INTO `activity_orders` VALUES (1, 'ACT202604270001', NULL, 39.90, 1, 1, 1, '2026-04-27 13:00:00');
INSERT INTO `activity_orders` VALUES (2, 'ACT202604270002', 'WX_ACT_202604270002', 119.80, 2, 2, 2, '2026-04-27 13:10:00');

-- ----------------------------
-- Table structure for activity_status
-- ----------------------------
DROP TABLE IF EXISTS `activity_status`;
CREATE TABLE `activity_status`  (
  `activity_status_id` int NOT NULL AUTO_INCREMENT COMMENT '活动状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`activity_status_id`) USING BTREE,
  UNIQUE INDEX `uk_activity_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_status
-- ----------------------------
INSERT INTO `activity_status` VALUES (1, '上架');
INSERT INTO `activity_status` VALUES (2, '下架');
INSERT INTO `activity_status` VALUES (3, '售罄');

-- ----------------------------
-- Table structure for activity_type
-- ----------------------------
DROP TABLE IF EXISTS `activity_type`;
CREATE TABLE `activity_type`  (
  `activity_type_id` int NOT NULL AUTO_INCREMENT COMMENT '活动类型ID',
  `type_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '类型名称',
  PRIMARY KEY (`activity_type_id`) USING BTREE,
  UNIQUE INDEX `uk_activity_type_name`(`type_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动类型表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_type
-- ----------------------------
INSERT INTO `activity_type` VALUES (2, '亲子研学');
INSERT INTO `activity_type` VALUES (1, '采摘体验');

-- ----------------------------
-- Table structure for activity_verification_record
-- ----------------------------
DROP TABLE IF EXISTS `activity_verification_record`;
CREATE TABLE `activity_verification_record`  (
  `record_id` bigint NOT NULL AUTO_INCREMENT COMMENT '核销记录ID',
  `activity_order_details_id` bigint NOT NULL COMMENT '活动订单明细ID',
  `verification_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '核销时间',
  PRIMARY KEY (`record_id`) USING BTREE,
  INDEX `idx_activity_verification_detail_id`(`activity_order_details_id` ASC) USING BTREE,
  INDEX `idx_activity_verification_time`(`verification_time` ASC) USING BTREE,
  CONSTRAINT `fk_activity_verification_record_detail` FOREIGN KEY (`activity_order_details_id`) REFERENCES `activity_order_detail` (`activity_order_details_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '活动核销记录表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of activity_verification_record
-- ----------------------------
INSERT INTO `activity_verification_record` VALUES (1, 2, '2026-04-27 16:00:00');

-- ----------------------------
-- Table structure for admin
-- ----------------------------
DROP TABLE IF EXISTS `admin`;
CREATE TABLE `admin`  (
  `admin_id` int NOT NULL AUTO_INCREMENT COMMENT '管理员ID',
  `user_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '管理员账号',
  `user_password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '密码',
  PRIMARY KEY (`admin_id`) USING BTREE,
  UNIQUE INDEX `uk_admin_user_no`(`user_no` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '管理员表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of admin
-- ----------------------------
INSERT INTO `admin` VALUES (1, 'admin', '123456');
INSERT INTO `admin` VALUES (2, 'farm_admin', '123456');

-- ----------------------------
-- Table structure for admin_operation_log
-- ----------------------------
DROP TABLE IF EXISTS `admin_operation_log`;
CREATE TABLE `admin_operation_log`  (
  `log_id` int NOT NULL AUTO_INCREMENT COMMENT '管理员操作日志ID',
  `user_id` int NOT NULL COMMENT '操作人ID',
  `operation_type` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '操作类型',
  `target_table` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '操作表名',
  `target_id` int NOT NULL COMMENT '操作对象ID',
  `operation_content` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '操作内容',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`log_id`) USING BTREE,
  INDEX `idx_admin_operation_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_admin_operation_create_time`(`create_time` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '管理员操作日志表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of admin_operation_log
-- ----------------------------
INSERT INTO `admin_operation_log` VALUES (1, 3, '新增', 'commodity', 1, '新增测试商品：甜养玉米500g', '2026-04-27 16:20:00');
INSERT INTO `admin_operation_log` VALUES (2, 3, '新增', 'activity', 1, '新增测试活动：周末草莓采摘体验', '2026-04-27 16:21:00');

-- ----------------------------
-- Table structure for carousel
-- ----------------------------
DROP TABLE IF EXISTS `carousel`;
CREATE TABLE `carousel`  (
  `carousel_id` bigint NOT NULL AUTO_INCREMENT COMMENT '轮播ID',
  `image_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '轮播图',
  `link_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '跳转链接',
  `position` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'home' COMMENT '位置 home/goods/acres',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`carousel_id`) USING BTREE,
  INDEX `idx_carousel_pos_sort`(`position` ASC, `sort_order` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '轮播图表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of carousel
-- ----------------------------
INSERT INTO `carousel` VALUES (1, '/images/farm/Farm_39.jpg', '/pages/farm-goods/farm-goods', 'home', 1, '2026-04-27 10:20:00');
INSERT INTO `carousel` VALUES (2, '/images/farm/Farm_7.jpg', '/pages/goods-detail/goods-detail?id=1', 'goods', 1, '2026-04-27 10:21:00');

-- ----------------------------
-- Table structure for commodity
-- ----------------------------
DROP TABLE IF EXISTS `commodity`;
CREATE TABLE `commodity`  (
  `commodity_id` int NOT NULL AUTO_INCREMENT COMMENT '商品ID',
  `spec_description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '规格描述',
  `in_stock` int NOT NULL DEFAULT 0 COMMENT '库存',
  `quantity` int NOT NULL DEFAULT 0 COMMENT '商品对应单位数量',
  `product_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '商品名称',
  `category_id` int NOT NULL COMMENT '分类ID',
  `image_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '主图',
  `unit_price` decimal(10, 2) NOT NULL DEFAULT 0.00 COMMENT '售价',
  `weight_text` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '规格重量文本',
  `storage_condition` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '储存条件',
  `unit_id` int NULL DEFAULT NULL COMMENT '单位ID',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '修改时间',
  `commodity_status_id` int NULL DEFAULT NULL COMMENT '商品状态ID',
  PRIMARY KEY (`commodity_id`) USING BTREE,
  INDEX `idx_commodity_category_id`(`category_id` ASC) USING BTREE,
  INDEX `idx_commodity_unit_id`(`unit_id` ASC) USING BTREE,
  INDEX `idx_commodity_status_id`(`commodity_status_id` ASC) USING BTREE,
  INDEX `idx_commodity_product_name`(`product_name` ASC) USING BTREE,
  CONSTRAINT `fk_commodity_category` FOREIGN KEY (`category_id`) REFERENCES `commodity_category` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_status` FOREIGN KEY (`commodity_status_id`) REFERENCES `commodity_status` (`commodity_status_id`) ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_unit` FOREIGN KEY (`unit_id`) REFERENCES `unit` (`unit_id`) ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity
-- ----------------------------
INSERT INTO `commodity` VALUES (1, '500g/份，软糯香甜', 1, 120, '甜养玉米500g', 1, '/images/farm/Farm_15.jpg', 8.90, '500g', '常温保存', 2, '2026-04-27 10:30:00', '2026-04-27 10:30:00', 1);
INSERT INTO `commodity` VALUES (2, '500g/份，农家新鲜土豆', 1, 200, '农家土豆500g', 1, '/images/farm/Farm_22.jpg', 5.90, '500g', '阴凉干燥处保存', 2, '2026-04-27 10:31:00', '2026-04-27 10:31:00', 1);
INSERT INTO `commodity` VALUES (3, '礼盒装，应季生态草莓', 1, 80, '生态草莓礼盒', 2, '/images/farm/Farm_6.jpg', 29.90, '1盒', '冷藏保存', 3, '2026-04-27 10:32:00', '2026-04-27 10:32:00', 1);
INSERT INTO `commodity` VALUES (4, '五谷杂粮组合礼盒', 1, 60, '五谷杂粮礼盒', 3, '/images/farm/Farm_48.jpg', 49.90, '1盒', '阴凉干燥处保存', 3, '2026-04-27 10:33:00', '2026-04-27 10:33:00', 1);

-- ----------------------------
-- Table structure for commodity_category
-- ----------------------------
DROP TABLE IF EXISTS `commodity_category`;
CREATE TABLE `commodity_category`  (
  `id` int NOT NULL AUTO_INCREMENT COMMENT '分类ID',
  `category_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '分类名称',
  `category_description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '分类描述',
  `category_status_id` int NULL DEFAULT NULL COMMENT '分类状态ID',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序号',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_category_name`(`category_name` ASC) USING BTREE,
  INDEX `idx_category_status_id`(`category_status_id` ASC) USING BTREE,
  INDEX `idx_category_sort_order`(`sort_order` ASC) USING BTREE,
  CONSTRAINT `fk_category_status` FOREIGN KEY (`category_status_id`) REFERENCES `commodity_category_status` (`category_status_id`) ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品分类表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_category
-- ----------------------------
INSERT INTO `commodity_category` VALUES (1, '新鲜蔬菜', '农场当天采摘的新鲜蔬菜', 1, 1);
INSERT INTO `commodity_category` VALUES (2, '时令水果', '应季水果与农场精选果品', 1, 2);
INSERT INTO `commodity_category` VALUES (3, '粮油干货', '农场谷物、杂粮和干货产品', 1, 3);

-- ----------------------------
-- Table structure for commodity_category_status
-- ----------------------------
DROP TABLE IF EXISTS `commodity_category_status`;
CREATE TABLE `commodity_category_status`  (
  `category_status_id` int NOT NULL AUTO_INCREMENT COMMENT '分类状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`category_status_id`) USING BTREE,
  UNIQUE INDEX `uk_category_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品分类状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_category_status
-- ----------------------------
INSERT INTO `commodity_category_status` VALUES (2, '停用');
INSERT INTO `commodity_category_status` VALUES (1, '启用');

-- ----------------------------
-- Table structure for commodity_material
-- ----------------------------
DROP TABLE IF EXISTS `commodity_material`;
CREATE TABLE `commodity_material`  (
  `material_id` bigint NOT NULL AUTO_INCREMENT COMMENT '素材ID',
  `commodity_id` int NOT NULL COMMENT '商品ID',
  `material_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '素材类型 0图 1详情图 2视频',
  `material_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '素材地址',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `sort_order` int NULL DEFAULT 0 COMMENT '排序',
  PRIMARY KEY (`material_id`) USING BTREE,
  INDEX `idx_commodity_material_commodity`(`commodity_id` ASC, `sort_order` ASC) USING BTREE,
  CONSTRAINT `fk_commodity_material_commodity` FOREIGN KEY (`commodity_id`) REFERENCES `commodity` (`commodity_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品素材表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_material
-- ----------------------------
INSERT INTO `commodity_material` VALUES (1, 1, 'image', '/images/farm/Farm_15.jpg', '2026-04-27 10:40:00', 1);
INSERT INTO `commodity_material` VALUES (2, 1, 'detail', '/images/farm/Farm_16.jpg', '2026-04-27 10:41:00', 2);
INSERT INTO `commodity_material` VALUES (3, 2, 'image', '/images/farm/Farm_22.jpg', '2026-04-27 10:42:00', 1);
INSERT INTO `commodity_material` VALUES (4, 3, 'image', '/images/farm/Farm_31.jpg', '2026-04-27 10:43:00', 1);
INSERT INTO `commodity_material` VALUES (5, 4, 'image', '/images/farm/Farm_48.jpg', '2026-04-27 10:44:00', 1);

-- ----------------------------
-- Table structure for commodity_order_detail
-- ----------------------------
DROP TABLE IF EXISTS `commodity_order_detail`;
CREATE TABLE `commodity_order_detail`  (
  `commodity_order_details_id` bigint NOT NULL AUTO_INCREMENT COMMENT '商品订单明细ID',
  `order_id` bigint NOT NULL COMMENT '订单ID',
  `commodity_id` int NOT NULL COMMENT '商品ID',
  `unit_price` decimal(10, 2) NOT NULL COMMENT '商品单价',
  `quantity` int NOT NULL COMMENT '订购数量',
  `subtotal_amount` decimal(10, 2) NOT NULL COMMENT '小计金额',
  `status_id` int NULL DEFAULT NULL COMMENT '商品状态ID',
  PRIMARY KEY (`commodity_order_details_id`) USING BTREE,
  INDEX `idx_commodity_order_detail_order_id`(`order_id` ASC) USING BTREE,
  INDEX `idx_commodity_order_detail_commodity_id`(`commodity_id` ASC) USING BTREE,
  INDEX `idx_commodity_order_detail_status_id`(`status_id` ASC) USING BTREE,
  CONSTRAINT `fk_commodity_order_detail_commodity` FOREIGN KEY (`commodity_id`) REFERENCES `commodity` (`commodity_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_order_detail_order` FOREIGN KEY (`order_id`) REFERENCES `commodity_orders` (`order_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品订单明细表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_order_detail
-- ----------------------------
INSERT INTO `commodity_order_detail` VALUES (1, 1, 1, 8.90, 2, 17.80, 1);
INSERT INTO `commodity_order_detail` VALUES (2, 1, 2, 5.90, 1, 5.90, 1);
INSERT INTO `commodity_order_detail` VALUES (3, 2, 3, 29.90, 1, 29.90, 3);

-- ----------------------------
-- Table structure for commodity_order_status
-- ----------------------------
DROP TABLE IF EXISTS `commodity_order_status`;
CREATE TABLE `commodity_order_status`  (
  `order_status_id` int NOT NULL AUTO_INCREMENT COMMENT '商品订单状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`order_status_id`) USING BTREE,
  UNIQUE INDEX `uk_commodity_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 8 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品订单状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_order_status
-- ----------------------------
INSERT INTO `commodity_order_status` VALUES (5, '已取消');
INSERT INTO `commodity_order_status` VALUES (4, '已完成');
INSERT INTO `commodity_order_status` VALUES (7, '已退款');
INSERT INTO `commodity_order_status` VALUES (1, '待付款');
INSERT INTO `commodity_order_status` VALUES (2, '待发货');
INSERT INTO `commodity_order_status` VALUES (3, '运输中');
INSERT INTO `commodity_order_status` VALUES (6, '退款中');

-- ----------------------------
-- Table structure for commodity_orders
-- ----------------------------
DROP TABLE IF EXISTS `commodity_orders`;
CREATE TABLE `commodity_orders`  (
  `order_id` bigint NOT NULL AUTO_INCREMENT COMMENT '商品订单ID',
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '平台订单号',
  `wx_pay_no` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信支付订单号',
  `total_amount` decimal(10, 2) NOT NULL COMMENT '订单总金额',
  `total_quantity` int NOT NULL COMMENT '订单商品总数',
  `order_status_id` int NOT NULL COMMENT '订单状态ID',
  `user_id` int NOT NULL COMMENT '下单人ID',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '下单时间',
  `address_id` bigint NOT NULL COMMENT '收货地址ID',
  `tracking_number` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '快递单号',
  `tracking_type_id` bigint NULL DEFAULT NULL COMMENT '物流类型ID',
  PRIMARY KEY (`order_id`) USING BTREE,
  UNIQUE INDEX `uk_commodity_orders_order_no`(`order_no` ASC) USING BTREE,
  INDEX `idx_commodity_orders_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_commodity_orders_status_id`(`order_status_id` ASC) USING BTREE,
  INDEX `idx_commodity_orders_address_id`(`address_id` ASC) USING BTREE,
  INDEX `idx_commodity_orders_tracking_type_id`(`tracking_type_id` ASC) USING BTREE,
  INDEX `idx_commodity_orders_create_time`(`create_time` ASC) USING BTREE,
  CONSTRAINT `fk_commodity_orders_address` FOREIGN KEY (`address_id`) REFERENCES `shipping_address` (`address_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_orders_commodity_order_status_4` FOREIGN KEY (`order_status_id`) REFERENCES `commodity_order_status` (`order_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_orders_tracking_type` FOREIGN KEY (`tracking_type_id`) REFERENCES `tracking_type` (`tracking_type_id`) ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_orders_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品订单表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_orders
-- ----------------------------
INSERT INTO `commodity_orders` VALUES (1, 'GOODS202604270001', NULL, 23.70, 3, 1, 1, '2026-04-27 12:00:00', 1, NULL, NULL);
INSERT INTO `commodity_orders` VALUES (2, 'GOODS202604270002', 'WX202604270002', 29.90, 1, 3, 2, '2026-04-27 12:10:00', 2, 'SF202604270001', 1);

-- ----------------------------
-- Table structure for commodity_status
-- ----------------------------
DROP TABLE IF EXISTS `commodity_status`;
CREATE TABLE `commodity_status`  (
  `commodity_status_id` int NOT NULL AUTO_INCREMENT COMMENT '商品状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`commodity_status_id`) USING BTREE,
  UNIQUE INDEX `uk_commodity_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_status
-- ----------------------------
INSERT INTO `commodity_status` VALUES (1, '上架');
INSERT INTO `commodity_status` VALUES (2, '下架');
INSERT INTO `commodity_status` VALUES (3, '售罄');

-- ----------------------------
-- Table structure for commodity_tag_detail
-- ----------------------------
DROP TABLE IF EXISTS `commodity_tag_detail`;
CREATE TABLE `commodity_tag_detail`  (
  `commodity_tag_relation_id` int NOT NULL AUTO_INCREMENT COMMENT '商品标签关联ID',
  `commodity_id` int NOT NULL COMMENT '商品ID',
  `tag_id` int NOT NULL COMMENT '标签ID',
  PRIMARY KEY (`commodity_tag_relation_id`) USING BTREE,
  UNIQUE INDEX `uk_commodity_tag`(`commodity_id` ASC, `tag_id` ASC) USING BTREE,
  INDEX `idx_commodity_tag_tag_id`(`tag_id` ASC) USING BTREE,
  CONSTRAINT `fk_commodity_tag_relation_commodity` FOREIGN KEY (`commodity_id`) REFERENCES `commodity` (`commodity_id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `fk_commodity_tag_relation_tag` FOREIGN KEY (`tag_id`) REFERENCES `tag` (`tag_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品标签中转表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of commodity_tag_detail
-- ----------------------------
INSERT INTO `commodity_tag_detail` VALUES (1, 1, 1);
INSERT INTO `commodity_tag_detail` VALUES (2, 1, 3);
INSERT INTO `commodity_tag_detail` VALUES (3, 2, 2);
INSERT INTO `commodity_tag_detail` VALUES (4, 3, 4);
INSERT INTO `commodity_tag_detail` VALUES (5, 4, 5);

-- ----------------------------
-- Table structure for dining_table
-- ----------------------------
DROP TABLE IF EXISTS `dining_table`;
CREATE TABLE `dining_table`  (
  `dining_table_id` bigint NOT NULL AUTO_INCREMENT COMMENT '桌位ID',
  `table_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '桌号',
  `seat_count` int NOT NULL DEFAULT 0 COMMENT '可坐人数',
  `table_status_id` int NOT NULL DEFAULT 1 COMMENT '桌位状态ID',
  `qrcode_image_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '桌位二维码图片地址',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`dining_table_id`) USING BTREE,
  UNIQUE INDEX `uk_dining_table_no`(`table_no` ASC) USING BTREE,
  INDEX `idx_dining_table_status`(`table_status_id` ASC) USING BTREE,
  CONSTRAINT `fk_dining_table_status` FOREIGN KEY (`table_status_id`) REFERENCES `dining_table_status_dict` (`table_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '桌位表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dining_table
-- ----------------------------
INSERT INTO `dining_table` VALUES (1, 'A001', 4, 1, '/images/qrcode/table_a001.png', '2026-04-27 11:00:00');
INSERT INTO `dining_table` VALUES (2, 'A002', 6, 2, '/images/qrcode/table_a002.png', '2026-04-27 11:01:00');

-- ----------------------------
-- Table structure for dining_table_status_dict
-- ----------------------------
DROP TABLE IF EXISTS `dining_table_status_dict`;
CREATE TABLE `dining_table_status_dict`  (
  `table_status_id` int NOT NULL AUTO_INCREMENT COMMENT '桌位状态ID',
  `status_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`table_status_id`) USING BTREE,
  UNIQUE INDEX `uk_dining_table_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '桌位状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dining_table_status_dict
-- ----------------------------
INSERT INTO `dining_table_status_dict` VALUES (2, '使用中');
INSERT INTO `dining_table_status_dict` VALUES (3, '停用');
INSERT INTO `dining_table_status_dict` VALUES (1, '空闲');

-- ----------------------------
-- Table structure for dish
-- ----------------------------
DROP TABLE IF EXISTS `dish`;
CREATE TABLE `dish`  (
  `dish_id` int NOT NULL AUTO_INCREMENT COMMENT '菜品ID',
  `dish_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '菜品名称',
  `dish_description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '菜品描述',
  `dish_price` decimal(10, 2) NOT NULL COMMENT '价格',
  `dish_category_id` int NOT NULL COMMENT '菜品分类ID',
  `image_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '图片地址',
  `attribute_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '属性名称',
  `status` int NOT NULL DEFAULT 1 COMMENT '上架状态 1上架 0下架',
  `limited_edition` int NOT NULL DEFAULT 0 COMMENT '限量供应',
  `dish_sold` int NOT NULL DEFAULT 0 COMMENT '已售数量',
  `dish_remaining_quantity` int NOT NULL DEFAULT 0 COMMENT '剩余数量',
  `user_purchase_limit` int NOT NULL DEFAULT 0 COMMENT '用户限购份数 0不限购',
  `dish_status_id` int NULL DEFAULT NULL COMMENT '菜品状态ID',
  `unit_id` int NULL DEFAULT NULL COMMENT '单位ID',
  PRIMARY KEY (`dish_id`) USING BTREE,
  INDEX `idx_dish_category_id`(`dish_category_id` ASC) USING BTREE,
  INDEX `idx_dish_status_id`(`dish_status_id` ASC) USING BTREE,
  INDEX `idx_dish_unit_id`(`unit_id` ASC) USING BTREE,
  INDEX `idx_dish_name`(`dish_name` ASC) USING BTREE,
  CONSTRAINT `fk_dish_dish_category` FOREIGN KEY (`dish_category_id`) REFERENCES `dish_category` (`dish_category_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_status` FOREIGN KEY (`dish_status_id`) REFERENCES `dish_status` (`dish_status_id`) ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_unit` FOREIGN KEY (`unit_id`) REFERENCES `unit` (`unit_id`) ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish
-- ----------------------------
INSERT INTO `dish` VALUES (1, '农家小炒肉', '农家风味小炒肉，口味鲜香', 28.00, 1, '/images/farm/Farm_14.jpg', '常规', 1, 0, 20, 80, 0, 1, 2);
INSERT INTO `dish` VALUES (2, '柴火土鸡饭', '柴火土鸡搭配农家米饭', 32.00, 2, '/images/farm/Farm_46.jpg', '常规', 1, 0, 15, 60, 0, 1, 2);
INSERT INTO `dish` VALUES (3, '鲜榨玉米汁', '农场玉米鲜榨饮品', 12.00, 3, '/images/farm/Farm_12.jpg', '常规', 1, 0, 30, 100, 0, 1, 2);

-- ----------------------------
-- Table structure for dish_category
-- ----------------------------
DROP TABLE IF EXISTS `dish_category`;
CREATE TABLE `dish_category`  (
  `dish_category_id` int NOT NULL AUTO_INCREMENT COMMENT '菜品分类ID',
  `dish_category_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '分类名称',
  `dish_sort_order` int NOT NULL DEFAULT 0 COMMENT '排序号',
  `dish_category_status_id` int NULL DEFAULT NULL COMMENT '菜品分类状态ID',
  PRIMARY KEY (`dish_category_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_category_name`(`dish_category_name` ASC) USING BTREE,
  INDEX `idx_dish_category_status_id`(`dish_category_status_id` ASC) USING BTREE,
  INDEX `idx_dish_category_sort_order`(`dish_sort_order` ASC) USING BTREE,
  CONSTRAINT `fk_dish_category_dish_category_status` FOREIGN KEY (`dish_category_status_id`) REFERENCES `dish_category_status` (`dish_category_status_id`) ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品分类表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_category
-- ----------------------------
INSERT INTO `dish_category` VALUES (1, '农家热菜', 1, 1);
INSERT INTO `dish_category` VALUES (2, '特色主食', 2, 1);
INSERT INTO `dish_category` VALUES (3, '田园饮品', 3, 1);

-- ----------------------------
-- Table structure for dish_category_status
-- ----------------------------
DROP TABLE IF EXISTS `dish_category_status`;
CREATE TABLE `dish_category_status`  (
  `dish_category_status_id` int NOT NULL AUTO_INCREMENT COMMENT '菜品分类状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`dish_category_status_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_category_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品分类状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_category_status
-- ----------------------------
INSERT INTO `dish_category_status` VALUES (2, '停用');
INSERT INTO `dish_category_status` VALUES (1, '启用');

-- ----------------------------
-- Table structure for dish_image
-- ----------------------------
DROP TABLE IF EXISTS `dish_image`;
CREATE TABLE `dish_image`  (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '图片ID',
  `dish_id` int NOT NULL COMMENT '菜品ID',
  `image_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '图片地址',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_dish_image_dish`(`dish_id` ASC, `sort_order` ASC) USING BTREE,
  CONSTRAINT `fk_dish_image_dish` FOREIGN KEY (`dish_id`) REFERENCES `dish` (`dish_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品图片表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_image
-- ----------------------------
INSERT INTO `dish_image` VALUES (1, 1, '/images/farm/Farm_14.jpg', 1);
INSERT INTO `dish_image` VALUES (2, 2, '/images/farm/Farm_46.jpg', 1);
INSERT INTO `dish_image` VALUES (3, 3, '/images/farm/Farm_12.jpg', 1);

-- ----------------------------
-- Table structure for dish_order_detail_status
-- ----------------------------
DROP TABLE IF EXISTS `dish_order_detail_status`;
CREATE TABLE `dish_order_detail_status`  (
  `detail_status_id` int NOT NULL AUTO_INCREMENT COMMENT '菜品状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '菜品状态名称',
  PRIMARY KEY (`detail_status_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品出餐状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_order_detail_status
-- ----------------------------
INSERT INTO `dish_order_detail_status` VALUES (2, '已出餐');
INSERT INTO `dish_order_detail_status` VALUES (3, '已取消');
INSERT INTO `dish_order_detail_status` VALUES (1, '待出餐');

-- ----------------------------
-- Table structure for dish_order_details
-- ----------------------------
DROP TABLE IF EXISTS `dish_order_details`;
CREATE TABLE `dish_order_details`  (
  `dish_order_details_id` bigint NOT NULL AUTO_INCREMENT COMMENT '订餐明细ID',
  `dish_order_id` bigint NOT NULL COMMENT '订餐订单ID',
  `dish_id` int NOT NULL COMMENT '菜品ID',
  `unit_price` decimal(10, 2) NOT NULL COMMENT '菜品单价',
  `quantity` int NOT NULL COMMENT '订购数量',
  `subtotal_amount` decimal(10, 2) NOT NULL COMMENT '小计金额',
  `status_id` int NULL DEFAULT NULL COMMENT '出餐状态ID',
  PRIMARY KEY (`dish_order_details_id`) USING BTREE,
  INDEX `idx_dish_order_details_order_id`(`dish_order_id` ASC) USING BTREE,
  INDEX `idx_dish_order_details_dish_id`(`dish_id` ASC) USING BTREE,
  INDEX `idx_dish_order_details_status_id`(`status_id` ASC) USING BTREE,
  CONSTRAINT `fk_dish_order_details_dish` FOREIGN KEY (`dish_id`) REFERENCES `dish` (`dish_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_order_details_dish_order_detail_status_3` FOREIGN KEY (`status_id`) REFERENCES `dish_order_detail_status` (`detail_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_order_details_order` FOREIGN KEY (`dish_order_id`) REFERENCES `dish_orders` (`order_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品订单明细表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_order_details
-- ----------------------------
INSERT INTO `dish_order_details` VALUES (1, 1, 1, 28.00, 1, 28.00, 1);
INSERT INTO `dish_order_details` VALUES (2, 1, 2, 32.00, 1, 32.00, 1);
INSERT INTO `dish_order_details` VALUES (3, 2, 3, 12.00, 1, 12.00, 2);

-- ----------------------------
-- Table structure for dish_order_status
-- ----------------------------
DROP TABLE IF EXISTS `dish_order_status`;
CREATE TABLE `dish_order_status`  (
  `order_status_id` int NOT NULL AUTO_INCREMENT COMMENT '餐品订单状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '状态名称',
  PRIMARY KEY (`order_status_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_order_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '餐品订单状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_order_status
-- ----------------------------
INSERT INTO `dish_order_status` VALUES (2, '已付款');
INSERT INTO `dish_order_status` VALUES (4, '已取消');
INSERT INTO `dish_order_status` VALUES (3, '已完成');
INSERT INTO `dish_order_status` VALUES (1, '待付款');

-- ----------------------------
-- Table structure for dish_orders
-- ----------------------------
DROP TABLE IF EXISTS `dish_orders`;
CREATE TABLE `dish_orders`  (
  `order_id` bigint NOT NULL AUTO_INCREMENT COMMENT '餐品订单ID',
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '平台订单号',
  `wx_pay_no` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信支付订单号',
  `total_amount` decimal(10, 2) NOT NULL COMMENT '订单总金额',
  `total_quantity` int NOT NULL COMMENT '订单菜品总数',
  `order_status_id` int NOT NULL COMMENT '订单状态ID',
  `user_id` int NOT NULL COMMENT '下单人ID',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '下单时间',
  `dining_table_id` bigint NOT NULL COMMENT '桌台ID',
  PRIMARY KEY (`order_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_orders_order_no`(`order_no` ASC) USING BTREE,
  INDEX `idx_dish_orders_status_id`(`order_status_id` ASC) USING BTREE,
  INDEX `idx_dish_orders_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_dish_orders_dining_table_id`(`dining_table_id` ASC) USING BTREE,
  INDEX `idx_dish_orders_create_time`(`create_time` ASC) USING BTREE,
  CONSTRAINT `fk_dish_orders_dining_table` FOREIGN KEY (`dining_table_id`) REFERENCES `dining_table` (`dining_table_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_orders_status` FOREIGN KEY (`order_status_id`) REFERENCES `dish_order_status` (`order_status_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_dish_orders_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品订单表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_orders
-- ----------------------------
INSERT INTO `dish_orders` VALUES (1, 'DISH202604270001', NULL, 60.00, 2, 1, 1, '2026-04-27 12:30:00', 1);
INSERT INTO `dish_orders` VALUES (2, 'DISH202604270002', 'WX_DISH_202604270002', 12.00, 1, 2, 2, '2026-04-27 12:40:00', 2);

-- ----------------------------
-- Table structure for dish_status
-- ----------------------------
DROP TABLE IF EXISTS `dish_status`;
CREATE TABLE `dish_status`  (
  `dish_status_id` int NOT NULL AUTO_INCREMENT COMMENT '菜品状态ID',
  `status_name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '菜品状态名称',
  PRIMARY KEY (`dish_status_id`) USING BTREE,
  UNIQUE INDEX `uk_dish_status_name`(`status_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '菜品状态表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of dish_status
-- ----------------------------
INSERT INTO `dish_status` VALUES (1, '上架');
INSERT INTO `dish_status` VALUES (2, '下架');
INSERT INTO `dish_status` VALUES (3, '售罄');

-- ----------------------------
-- Table structure for role
-- ----------------------------
DROP TABLE IF EXISTS `role`;
CREATE TABLE `role`  (
  `role_id` int NOT NULL AUTO_INCREMENT COMMENT '角色ID',
  `role_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '角色名称',
  PRIMARY KEY (`role_id`) USING BTREE,
  UNIQUE INDEX `uk_role_name`(`role_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '角色表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of role
-- ----------------------------
INSERT INTO `role` VALUES (2, '员工');
INSERT INTO `role` VALUES (1, '普通用户');

-- ----------------------------
-- Table structure for shipping_address
-- ----------------------------
DROP TABLE IF EXISTS `shipping_address`;
CREATE TABLE `shipping_address`  (
  `address_id` bigint NOT NULL AUTO_INCREMENT COMMENT '收货地址ID',
  `user_id` int NOT NULL COMMENT '用户ID',
  `contact_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '联系人姓名',
  `contact_phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '联系电话',
  `province` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '省份',
  `city` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '市',
  `municipal_district` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '区/县',
  `addres` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '详细地址',
  `is_default` tinyint(1) NOT NULL DEFAULT 0 COMMENT '是否默认地址',
  PRIMARY KEY (`address_id`) USING BTREE,
  INDEX `idx_shipping_address_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_shipping_address_default`(`user_id` ASC, `is_default` ASC) USING BTREE,
  CONSTRAINT `fk_shipping_address_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '收货地址表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of shipping_address
-- ----------------------------
INSERT INTO `shipping_address` VALUES (1, 1, '张三', '13800000001', '广东省', '广州市', '白云区', '三元里测试地址1号', 1);
INSERT INTO `shipping_address` VALUES (2, 2, '李四', '13800000002', '广东省', '广州市', '天河区', '珠江新城测试地址2号', 1);

-- ----------------------------
-- Table structure for shipping_cart
-- ----------------------------
DROP TABLE IF EXISTS `shipping_cart`;
CREATE TABLE `shipping_cart`  (
  `shipping_cart_id` int NOT NULL AUTO_INCREMENT COMMENT '购物车ID',
  `user_id` int NOT NULL COMMENT '用户ID',
  `cart_item_type` int NOT NULL COMMENT '购物车项目类型 1商品 2菜品',
  `dish_id` int NULL DEFAULT NULL COMMENT '菜品表ID',
  `commodity_id` int NULL DEFAULT NULL COMMENT '商品表ID',
  `cart_quantity` int NOT NULL DEFAULT 1 COMMENT '购买数量',
  PRIMARY KEY (`shipping_cart_id`) USING BTREE,
  INDEX `idx_shipping_cart_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_shipping_cart_type_item`(`cart_item_type` ASC, `dish_id` ASC, `commodity_id` ASC) USING BTREE,
  INDEX `idx_shipping_cart_user_type_item`(`user_id` ASC, `cart_item_type` ASC, `dish_id` ASC, `commodity_id` ASC) USING BTREE,
  INDEX `fk_shipping_cart_dish_2`(`dish_id` ASC) USING BTREE,
  INDEX `fk_shipping_cart_commodity_3`(`commodity_id` ASC) USING BTREE,
  CONSTRAINT `fk_shipping_cart_commodity_3` FOREIGN KEY (`commodity_id`) REFERENCES `commodity` (`commodity_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_shipping_cart_dish_2` FOREIGN KEY (`dish_id`) REFERENCES `dish` (`dish_id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_shipping_cart_user_3` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '购物车表（多业务通用，universal_id 不设置外键）' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of shipping_cart
-- ----------------------------
INSERT INTO `shipping_cart` VALUES (1, 1, 1, NULL, 1, 2);
INSERT INTO `shipping_cart` VALUES (2, 1, 1, NULL, 3, 1);
INSERT INTO `shipping_cart` VALUES (3, 2, 1, NULL, 4, 1);
INSERT INTO `shipping_cart` VALUES (4, 1, 2, 1, NULL, 1);
INSERT INTO `shipping_cart` VALUES (5, 2, 2, 3, NULL, 2);

-- ----------------------------
-- Table structure for stock_log
-- ----------------------------
DROP TABLE IF EXISTS `stock_log`;
CREATE TABLE `stock_log`  (
  `stock_log_id` int NOT NULL AUTO_INCREMENT COMMENT '库存日志ID',
  `commodity_id` int NOT NULL COMMENT '商品ID',
  `change_quantity` int NOT NULL COMMENT '库存变更数量',
  `before_stock` int NOT NULL COMMENT '变更前库存',
  `after_stock` int NOT NULL COMMENT '变更后库存',
  `user_id` int NOT NULL COMMENT '操作人用户ID',
  `create_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`stock_log_id`) USING BTREE,
  INDEX `idx_stock_log_commodity_id`(`commodity_id` ASC) USING BTREE,
  INDEX `idx_stock_log_user_id`(`user_id` ASC) USING BTREE,
  INDEX `idx_stock_log_create_time`(`create_time` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '库存修改日志表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of stock_log
-- ----------------------------
INSERT INTO `stock_log` VALUES (1, 1, 120, 0, 120, 3, '2026-04-27 16:10:00');
INSERT INTO `stock_log` VALUES (2, 2, 200, 0, 200, 3, '2026-04-27 16:11:00');

-- ----------------------------
-- Table structure for tag
-- ----------------------------
DROP TABLE IF EXISTS `tag`;
CREATE TABLE `tag`  (
  `tag_id` int NOT NULL AUTO_INCREMENT COMMENT '标签ID',
  `tag_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '标签名称',
  PRIMARY KEY (`tag_id`) USING BTREE,
  UNIQUE INDEX `uk_tag_name`(`tag_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '标签表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of tag
-- ----------------------------
INSERT INTO `tag` VALUES (2, '农场优选');
INSERT INTO `tag` VALUES (1, '新鲜直采');
INSERT INTO `tag` VALUES (3, '热销');
INSERT INTO `tag` VALUES (4, '绿色健康');
INSERT INTO `tag` VALUES (5, '限时优惠');

-- ----------------------------
-- Table structure for tracking_type
-- ----------------------------
DROP TABLE IF EXISTS `tracking_type`;
CREATE TABLE `tracking_type`  (
  `tracking_type_id` bigint NOT NULL AUTO_INCREMENT COMMENT '物流类型ID',
  `tracking_type_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '物流类型名称',
  PRIMARY KEY (`tracking_type_id`) USING BTREE,
  UNIQUE INDEX `uk_tracking_type_name`(`tracking_type_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '物流类型表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of tracking_type
-- ----------------------------
INSERT INTO `tracking_type` VALUES (2, '邮政快递');
INSERT INTO `tracking_type` VALUES (1, '顺丰速运');

-- ----------------------------
-- Table structure for unit
-- ----------------------------
DROP TABLE IF EXISTS `unit`;
CREATE TABLE `unit`  (
  `unit_id` int NOT NULL AUTO_INCREMENT COMMENT '单位ID',
  `unit_name` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '单位名称',
  `unit_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '单位编码',
  `is_enabled` tinyint NOT NULL DEFAULT 1 COMMENT '1启用 0禁用',
  PRIMARY KEY (`unit_id`) USING BTREE,
  UNIQUE INDEX `uk_unit_code`(`unit_code` ASC) USING BTREE,
  UNIQUE INDEX `uk_unit_name`(`unit_name` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '单位表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of unit
-- ----------------------------
INSERT INTO `unit` VALUES (1, '斤', 'jin', 1);
INSERT INTO `unit` VALUES (2, '份', 'portion', 1);
INSERT INTO `unit` VALUES (3, '盒', 'box', 1);
INSERT INTO `unit` VALUES (4, '亩', 'mu', 1);
INSERT INTO `unit` VALUES (5, '人', 'person', 1);

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`  (
  `user_id` int NOT NULL AUTO_INCREMENT COMMENT '用户ID',
  `user_guid` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '用户唯一标识',
  `phone_number` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '手机号',
  `register_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '注册时间',
  `wx_openid` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信OpenID',
  `wx_image` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信头像',
  `wx_nickname` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信昵称',
  `real_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '用户姓名',
  `password_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '密码哈希值',
  `gender` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '保密' COMMENT '性别',
  `role_id` int NOT NULL COMMENT '角色ID',
  PRIMARY KEY (`user_id`) USING BTREE,
  UNIQUE INDEX `uk_user_guid`(`user_guid` ASC) USING BTREE,
  UNIQUE INDEX `uk_user_wx_openid`(`wx_openid` ASC) USING BTREE,
  INDEX `idx_user_role_id`(`role_id` ASC) USING BTREE,
  INDEX `idx_user_phone_number`(`phone_number` ASC) USING BTREE,
  CONSTRAINT `fk_user_role` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of user
-- ----------------------------
INSERT INTO `user` VALUES (1, 'user-guid-0001', '13800000001', '2026-04-27 10:00:00', 'test_openid_0001', '/images/avatar/user1.png', '农场小明', '张三', '', '男', 1);
INSERT INTO `user` VALUES (2, 'user-guid-0002', '13800000002', '2026-04-27 10:05:00', 'test_openid_0002', '/images/avatar/user2.png', '田园小红', '李四', '', '女', 1);
INSERT INTO `user` VALUES (3, 'user-guid-admin', '13800000003', '2026-04-27 10:10:00', 'test_openid_admin', '/images/avatar/admin.png', '后台管理员', '管理员', '', '保密', 2);
INSERT INTO `user` VALUES (4, '56d94ea620574391b2b4acfc5ce52457', '17328667680', '2026-04-27 18:17:19', 'opRJJ1-oX9T6jMGp3iCtSl8j6Gdk', '', '微信用户', '', '', '保密', 1);

-- ----------------------------
-- Table structure for videos
-- ----------------------------
DROP TABLE IF EXISTS `videos`;
CREATE TABLE `videos`  (
  `video_id` bigint NOT NULL AUTO_INCREMENT COMMENT '视频ID',
  `video_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '视频URL',
  `position` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'home' COMMENT '位置 home/goods/acres',
  `sort_order` int NOT NULL DEFAULT 0 COMMENT '排序',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`video_id`) USING BTREE,
  INDEX `idx_videos_pos_sort`(`position` ASC, `sort_order` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '视频表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of videos
-- ----------------------------
INSERT INTO `videos` VALUES (1, '/videos/farm_intro.mp4', 'home', 1, '2026-04-27 10:22:00');
INSERT INTO `videos` VALUES (2, '/videos/farm_intro.mp4', 'goods', 1, '2026-04-27 10:23:00');

SET FOREIGN_KEY_CHECKS = 1;
