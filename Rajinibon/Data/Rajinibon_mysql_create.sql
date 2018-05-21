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
	`student_check_time_id` int NOT NULL,
	`status` varchar(20),
	`sent_time` DATETIME NOT NULL,
	PRIMARY KEY (`id`)
);

ALTER TABLE `students_sent_message` ADD CONSTRAINT `students_sent_message_fk0` FOREIGN KEY (`student_check_time_id`) REFERENCES `students_check_time`(`id`);

