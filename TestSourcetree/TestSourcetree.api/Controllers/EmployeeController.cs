using Mono_projekt.webapi.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using HttpDeleteAttribute = System.Web.Http.HttpDeleteAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpPutAttribute = System.Web.Http.HttpPutAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

namespace Mono_projekt.webapi.Controllers
{
    public class EmployeeController : ApiController
    {
        private string connectionString;

        public EmployeeController()
        {
            connectionString = "Server=localhost;Port=5432;Database=HairSalon;User Id=postgres;Password=postgres;";
        }
        [HttpPost]
        public HttpResponseMessage InsertEmployee([FromBody] CreateEmployeeRest createEmployeeRest)
        {
            try
            {
                var connection = new NpgsqlConnection(connectionString);
                var command = new NpgsqlCommand("INSERT INTO Employee (Id, FirstName, LastName) VALUES (@id, @firstName, @lastName)", connection);
                using (connection)
                {
                    connection.Open();
                    Employee employee = new Employee(Guid.NewGuid(), createEmployeeRest.FirstName, createEmployeeRest.LastName);

                    command.Parameters.AddWithValue("@id", employee.Id);
                    command.Parameters.AddWithValue("@firstName", employee.FirstName);
                    command.Parameters.AddWithValue("@lastName", employee.LastName);
                    int affectedRows = command.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, employee);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Employee Didnt Create");

            }
        }

        [HttpDelete]
        [Route("api/employee/{Id}")]
        public HttpResponseMessage DeleteEmployee(Guid Id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string checkSql = "SELECT COUNT(*) FROM Employee WHERE Id = @id";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("id", Id);
                        int employeeCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (employeeCount == 0)
                        {
                            var response = Request.CreateResponse(HttpStatusCode.NotFound, "Employee not found");
                            return response;
                        }
                    }
                    string deleteSql = "DELETE FROM Employee WHERE Id = @id";
                    using (NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("id", Id);
                        deleteCommand.Connection = connection;
                        int rowsAffected = deleteCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Employee deleted");
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to delete employee");
                        }
                    }
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something went wrong");
            }
        }

        [HttpPut]
        [Route("api/employee/{Id}")]
        public HttpResponseMessage Update(Guid id, [FromBody] UpdateEmployeeRest updateEmployeeRest)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string checkSql = "SELECT COUNT(*) FROM Employee WHERE Id = @id";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("id", id);
                        int employeeCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (employeeCount == 0)
                        {
                            var response = Request.CreateResponse(HttpStatusCode.NotFound, "Employee not found");
                            return response;
                        }
                    }

                    string updateSql = "UPDATE Employee SET FirstName = @firstName, LastName = @lastName WHERE Id = @id";
                    using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("id", id);
                        updateCommand.Parameters.AddWithValue("firstName", updateEmployeeRest.FirstName);
                        updateCommand.Parameters.AddWithValue("lastName", updateEmployeeRest.LastName);
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Employee updated");
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to update employee");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/employee/{Id}")]
        public HttpResponseMessage GetEmployee(Guid Id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT Id, FirstName, LastName FROM Employee WHERE Id = @id";
                    using (NpgsqlCommand command = new NpgsqlCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("id", Id);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);
                                string firstName = reader.GetString(1);
                                string lastName = reader.GetString(2);

                                Employee employee = new Employee(id, firstName, lastName);

                                var response = Request.CreateResponse(HttpStatusCode.OK, employee);
                                return response;
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Employee not found");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetAllEmployees()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT Id, FirstName, LastName FROM Employee";
                    using (NpgsqlCommand command = new NpgsqlCommand(selectSql, connection))
                    {
                        List<Employee> employees = new List<Employee>();
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);
                                string firstName = reader.GetString(1);
                                string lastName = reader.GetString(2);
                                Employee employee = new Employee(id, firstName, lastName);
                                employees.Add(employee);
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, employees);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public int ExecuteScalar(string sql, NpgsqlParameter[] parameters)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 0;
        }
}