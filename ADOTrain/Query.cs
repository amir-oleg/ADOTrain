using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOTrain
{
    class Query
    {
        private Repository _repository = new Repository();

        public void InitDb()
        {
            _repository.GetConnection(c =>
            {
                var transaction = c.BeginTransaction();
                try
                {
                    using (var command = new SqlCommand())
                    {
                        command.Transaction = transaction;
                        command.Connection = c;
                        command.CommandText = "CREATE TABLE Students (Name varchar(255))";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SqlCommand())
                    {
                        command.Transaction = transaction;
                        command.Connection = c;
                        command.CommandText = "CREATE TABLE Lecture (Date DATE,Topic varchar(255))";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SqlCommand())
                    {
                        command.Transaction = transaction;
                        command.Connection = c;
                        command.CommandText = "CREATE TABLE Attendance (LectureDate DATE, StudentName varchar(255), Mark INT)";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SqlCommand())
                    {
                        command.Transaction = transaction;
                        command.Connection = c;
                        command.CommandText =
                            "CREATE PROCEDURE MarkAttendance @StudentName varchar(255), @LectureDate DATE, @Mark INT AS INSERT INTO Attendance (StudentName, LectureDate, Mark) VALUES (@StudentName, @LectureDate, @Mark)";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }

                return true;
            });
        }

        public int AddLecture(DateTime date, string topic)
        {
            return _repository.GetConnection(c =>
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = c;
                    command.CommandText = "INSERT INTO Lecture (Date, Topic) Values(@Date,@Topic)";
                    command.CommandType = CommandType.Text;
                    var dateParam = command.CreateParameter();
                    dateParam.ParameterName = "@Date";
                    dateParam.SqlDbType = SqlDbType.Date;
                    dateParam.Direction = ParameterDirection.Input;
                    dateParam.Value = date;
                    command.Parameters.Add(dateParam);
                    var topicParam = command.CreateParameter();
                    topicParam.ParameterName = "@Topic";
                    topicParam.SqlDbType = SqlDbType.VarChar;
                    topicParam.Size = 255;
                    topicParam.Direction = ParameterDirection.Input;
                    topicParam.Value = topic;
                    command.Parameters.Add(topicParam);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public int AddStudent(string name)
        {
            return _repository.GetConnection(c =>
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = c;
                    command.CommandText = "INSERT INTO Students (Name) Values(@Name)";
                    command.CommandType = CommandType.Text;
                    var nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@Name";
                    nameParam.SqlDbType = SqlDbType.VarChar;
                    nameParam.Size = 255;
                    nameParam.Direction = ParameterDirection.Input;
                    nameParam.Value = name;
                    command.Parameters.Add(nameParam);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public int AddAttend(string name, DateTime date, int mark)
        {
            return _repository.GetConnection(c =>
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = c;
                    command.CommandText = "MarkAttendance";//"INSERT INTO Attendance (LectureDate, StudentName, Mark) Values(@Date,@Name,@Mark)";
                    command.CommandType = CommandType.StoredProcedure;
                    var dateParam = command.CreateParameter();
                    dateParam.ParameterName = "@LectureDate";
                    dateParam.SqlDbType = SqlDbType.Date;
                    dateParam.Direction = ParameterDirection.Input;
                    dateParam.Value = date;
                    command.Parameters.Add(dateParam);
                    var nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@StudentName";
                    nameParam.SqlDbType = SqlDbType.VarChar;
                    nameParam.Size = 255;
                    nameParam.Direction = ParameterDirection.Input;
                    nameParam.Value = name;
                    command.Parameters.Add(nameParam);
                    var markParam = command.CreateParameter();
                    markParam.ParameterName = "@Mark";
                    markParam.SqlDbType = SqlDbType.Int;
                    markParam.Direction = ParameterDirection.Input;
                    markParam.Value = mark;
                    command.Parameters.Add(markParam);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public void PrintReport()
        {
            _repository.GetConnection(c =>
            {
                var dataSet = new DataSet();
                using (var adapter = new SqlDataAdapter("SELECT * FROM Lecture", c))
                    adapter.Fill(dataSet, "Lecture");
                using (var adapter = new SqlDataAdapter("SELECT * FROM Attendance", c))// left join Lecture on Attendance.LectureDate=Lecture.Date
                    adapter.Fill(dataSet,"Attendance");
                using (var adapter = new SqlDataAdapter("SELECT * FROM Students left join Attendance on Attendance.StudentName=Students.Name", c))
                    adapter.Fill(dataSet, "Students");
                var attendance = dataSet.Tables["Attendance"];
                var students = dataSet.Tables["Students"];
                var lecture = dataSet.Tables["Lecture"];
                
                foreach (DataRow row in lecture.Rows)
                {
                    foreach (DataColumn column in lecture.Columns)
                    {
                        Console.Write($"    {row[column]}");
                    }
                    Console.WriteLine();
                    foreach (DataRow rowAtt in attendance.Rows)
                    {
                        if(rowAtt[attendance.Columns.IndexOf("LectureDate")].ToString()==row[lecture.Columns.IndexOf("Date")].ToString())
                            Console.WriteLine(rowAtt[attendance.Columns.IndexOf("StudentName")]);
                    }
                }
                Console.WriteLine("Не были на лекциях:");
                foreach (DataRow row in students.Rows)
                {
                    if(string.IsNullOrEmpty(row[students.Columns.IndexOf("LectureDate")].ToString()))
                        Console.WriteLine(row["Name"].ToString());
                }

                return true;
            });

            //using (var command = new SqlCommand("SELECT * FROM Attendance right join Students on Attendance.StudentName=Students.Name", c))
            //{
            //    var reader = command.ExecuteReader();
            //    while (reader.HasRows)
            //    {
            //        Console.WriteLine($"{reader.GetName(0)}\t{reader.GetName(1)}\t{reader.GetName(2)}");
            //        while (reader.Read())
            //        {
            //            Console.WriteLine(string.IsNullOrEmpty(reader[1].ToString())
            //                ? $"  {reader[3]} не посетил ни одной лекции"
            //                : $"  {reader[0]}; {reader[1]}; {reader[2]}");
            //        }
            //        reader.NextResult();
            //    }

            //    reader.Close();
            //}
            //return true;

        }
    }
}
