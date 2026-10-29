-- FoodTasteApp local demo database. Additive; never DROP/TRUNCATE/DELETE.
CREATE DATABASE IF NOT EXISTS `tasteapp` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `tasteapp`;
CREATE TABLE IF NOT EXISTS `user` (
  `Id` VARCHAR(64) NOT NULL PRIMARY KEY, `Pwd` VARCHAR(255) NOT NULL,
  `NickName` VARCHAR(80) NOT NULL UNIQUE, `Age` INT NOT NULL DEFAULT 0,
  `Region` VARCHAR(100) NOT NULL DEFAULT '', `ProImg` LONGBLOB NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `fooddata` (
  `FoodNum` INT NOT NULL PRIMARY KEY, `FoodName` VARCHAR(255) NOT NULL,
  `FoodKcal` VARCHAR(64) NULL, `FoodGram` VARCHAR(128) NULL,
  KEY `idx_fooddata_name` (`FoodName`(120))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `foodtastescore` (
  `id` VARCHAR(64) NOT NULL, `foodnum` INT NOT NULL,
  `foodscore` INT NOT NULL, `foodname` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`id`,`foodnum`),
  KEY `idx_rating_food` (`foodnum`,`foodscore`),
  KEY `idx_rating_name` (`foodname`(120))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `search_history` (
  `history_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `user_id` VARCHAR(64) NOT NULL, `tag` VARCHAR(16) NOT NULL,
  `foodname` VARCHAR(255) NOT NULL, `search_date` DATE NOT NULL,
  `eaten_tag` VARCHAR(16) NOT NULL DEFAULT '',
  UNIQUE KEY `uq_search_entry` (`user_id`,`tag`,`foodname`(100),`search_date`,`eaten_tag`),
  KEY `idx_search_tag_date` (`tag`,`search_date`),
  KEY `idx_search_user_date` (`user_id`,`search_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `comments` (
  `comment_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `foodname` VARCHAR(255) NOT NULL, `id` VARCHAR(64) NOT NULL,
  `replydate` DATETIME NOT NULL, `content` TEXT NOT NULL,
  KEY `idx_comments_food` (`foodname`(120),`replydate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `friend_requests` (
  `request_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `sender_id` VARCHAR(64) NOT NULL, `receiver_id` VARCHAR(64) NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'pending',
  KEY `idx_friend_req_receiver` (`receiver_id`,`status`),
  KEY `idx_friend_req_sender` (`sender_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `friendships` (
  `friendship_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `user_id_1` VARCHAR(64) NOT NULL, `user_id_2` VARCHAR(64) NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'acceptance',
  KEY `idx_friendships_user1` (`user_id_1`,`status`),
  KEY `idx_friendships_user2` (`user_id_2`,`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `messages` (
  `message_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `sender_id` VARCHAR(64) NOT NULL, `receiver_id` VARCHAR(64) NOT NULL,
  `message` TEXT NOT NULL, `send_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY `idx_messages_pair` (`sender_id`,`receiver_id`,`send_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `chat` (
  `chat_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `sender_id` VARCHAR(64) NOT NULL, `receiver_id` VARCHAR(64) NOT NULL,
  `message` TEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS `game_scores` (
  `player_id` VARCHAR(64) NOT NULL PRIMARY KEY,
  `score` INT NOT NULL DEFAULT 0,
  KEY `idx_game_rank` (`score`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
