CREATE TABLE `students_check_time` (
	`id` int NOT NULL AUTO_INCREMENT,
	`cuser_id` bigint,
	`emp_id` varchar(10) NOT NULL,
	`emp_name` varchar(255) NOT NULL,
	`chk_time` DATETIME NOT NULL,
	PRIMARY KEY (`id`)
);

CREATE TABLE `students_sent_message` (
	`id` int NOT NULL AUTO_INCREMENT,
	`emp_id` varchar(10) NOT NULL,
	`sent_type` varchar(50) NOT NULL,
	`status` varchar(255),
	`sent_time` DATETIME NOT NULL,
	PRIMARY KEY (`id`)
);
