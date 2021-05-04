using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ADOTrain.Dtos;
using ADOTrain.Entities;

namespace ADOTrain
{
    public class Repository
    {
        private static readonly string ConnectionString = Properties.Resource.ConnectionString;
        private const string CreateTableStudents = "CREATE TABLE Students (Name varchar(255))";
        private const string CreateTableLecture = "CREATE TABLE Lecture (Date DATE,Topic varchar(255))";
        private const string CreateTableAttendance = "CREATE TABLE Attendance (LectureDate DATE, StudentName varchar(255), Mark INT)";
        private const string CreateProcedureMarkAttendance =
            "CREATE PROCEDURE MarkAttendance @StudentName varchar(255), @LectureDate DATE, @Mark INT AS INSERT INTO Attendance (StudentName, LectureDate, Mark) VALUES (@StudentName, @LectureDate, @Mark)";
        private const string InsertIntoLecture = "INSERT INTO Lecture (Date, Topic) Values(@Date,@Topic)";
        private const string InsertIntoStudents = "INSERT INTO Students(Name) Values(@Name)";
        private const string ProcedureMarkAttendance = "MarkAttendance";
        private const string SelectAllFromLecture = "SELECT * FROM Lecture";
        private const string SelectAllFromAttendance = "SELECT * FROM Attendeance";
        private const string SelectAllFromStudents = "SELECT * FROM Students";


        private static T GetConnection<T>(Func<SqlConnection, T> getData)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return getData(connection);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }

        private static T CreateCommand<T>(IDbConnection connection, string commandText, int commandTimeout = -1,
            CommandType commandType = CommandType.Text, IDbTransaction transaction = null) where T : IDbCommand, new()
        {
            var command = new T {Connection = connection, CommandText = commandText, CommandType = commandType};

            if (commandTimeout > 0)
            {
                command.CommandTimeout = commandTimeout;
            }

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            return command;
        }

        private static void CreateParameter(SqlCommand command, string parameterName, SqlDbType sqlDbType = default,
            int size = -1, ParameterDirection direction = default, object value = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = sqlDbType;
            parameter.Direction = direction;
            parameter.Value = value;

            if (size > 0)
            {
                parameter.Size = size;
            }
            
            command.Parameters.Add(parameter);
        }

        public void InitDb()
        {
            GetConnection(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command =
                            CreateCommand<SqlCommand>(connection, CreateTableStudents, transaction: transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        using (var command = 
                            CreateCommand<SqlCommand>(connection, CreateTableLecture, transaction: transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        using (var command = 
                            CreateCommand<SqlCommand>(connection, CreateTableAttendance, transaction: transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        using (var command =
                            CreateCommand<SqlCommand>(connection, CreateProcedureMarkAttendance, transaction: transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }

                    return true;
                }
            });
        }

        public int AddLecture(DateTime date, string topic)
        {
            return GetConnection(connection =>
            {
                using (var command = CreateCommand<SqlCommand>(connection, InsertIntoLecture))
                {
                    CreateParameter(command, "@Date", SqlDbType.Date, direction: ParameterDirection.Input, value: date);
                    CreateParameter(command, "@Topic", SqlDbType.VarChar, 255, ParameterDirection.Input, topic);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public int AddStudent(string name)
        {
            return GetConnection(connection =>
            {
                using (var command = CreateCommand<SqlCommand>(connection, InsertIntoStudents))
                {
                    CreateParameter(command, "@Name", SqlDbType.VarChar, 255, ParameterDirection.Input, name);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public int AddAttend(string name, DateTime date, int mark)
        {
            return GetConnection(connection =>
            {
                using (var command = CreateCommand<SqlCommand>(connection, ProcedureMarkAttendance, commandType:CommandType.StoredProcedure))
                {
                    CreateParameter(command, "@LectureDate", SqlDbType.Date, direction: ParameterDirection.Input, value: date);
                    CreateParameter(command, "@StudentName", SqlDbType.VarChar, 255, ParameterDirection.Input, name);
                    CreateParameter(command, "@Mark", SqlDbType.Int, direction: ParameterDirection.Input, value: mark);
                    return command.ExecuteNonQuery();
                }
            });
        }

        public (List<AttendanceDto>, List<string>) GetReport()
        {
            return GetConnection(connection =>
            {
                var lectures = new List<Lecture>();
                var attendance = new List<Attendance>();
                var students = new List<string>();
                var result = new List<AttendanceDto>();
                using (var command = CreateCommand<SqlCommand>(connection, SelectAllFromLecture))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[0] is DateTime date))
                            {
                                continue;
                            }
                            lectures.Add(new Lecture {Date = date, Topic = reader[1].ToString()});
                        }

                        reader.Close();
                    }
                }

                using (var command = CreateCommand<SqlCommand>(connection, SelectAllFromAttendance))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[0] is DateTime date) || !(reader[2] is int mark)) //DateTime.TryParse || int.TryParse
                            {
                                continue;
                            }
                            attendance.Add(new Attendance {LectureDate = date, StudentName = reader[1].ToString(), Mark = mark});
                        }

                        reader.Close();
                    }
                }

                using (var command = CreateCommand<SqlCommand>(connection, SelectAllFromStudents))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(reader[0].ToString());
                        }

                        reader.Close();
                    }
                }

                foreach (var lecture in lectures)
                {
                    var dto = new AttendanceDto{LectureDate = lecture.Date,Topic = lecture.Topic, Students = new List<string>()};
                    var studentOnLecture = attendance.Where(att => att.LectureDate == lecture.Date)
                        .Select(att => att.StudentName).ToList();
                    dto.Students.AddRange(studentOnLecture);
                    foreach (var student in studentOnLecture)
                    {
                        students.Remove(student);
                    }
                    result.Add(dto);
                }

                return (result,students);
            });

        }
    }
}
