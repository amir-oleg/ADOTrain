ADOTrain.exe функциональность:

	ADOTrain -init: инициализировать базу данных
		создать таблицу 
			студентов (Students {Name})
			лекций (Lecture {Date, Topic})
			посещаемости (Attendance {LectureDate, StudentName, Mark})
		создать хранимую процедуру, отмечающую определенного студента на лекции
			MarkAttendance @StudentName, @LectureDate, @Mark
	ADOTrain -lecture <DATE> <TOPIC>: добавить лекцию в таблицу лекций (по дате)
		например: sc -lecture 18.12.2017 ADONET
	
	ADOTrain -student <NAME>: добавить студента в таблицу студентов
		например: sc -student Ivan

	ADOTrain -attend <STUDENT_NAME> <DATE> <MARK>: добавить запись о посещении студента в таблице посещаемости
		например: sc -attend Ivan 18.12.2017 5
		
	ADOTrain -report: вывести отчет о посещаемости
		*выводить Topic лекции
		*если студент не посетил ни одной лекции, все равно выводить его имя
		*если лекцию никто не посеил, все равно выводить дату и тему
