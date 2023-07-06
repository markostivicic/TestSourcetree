using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Mono_projekt.webapi.Models;
using HttpDeleteAttribute = System.Web.Http.HttpDeleteAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpPutAttribute = System.Web.Http.HttpPutAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

namespace Mono_projekt.webapi.Controllers
{
    public class CustomerController : ApiController
    {
        private string connectionString;

        public CustomerController()
        {
            connectionString = "Server=localhost;Port=5432;Database=HairSalon;User Id=postgres;Password=postgres;";
        }
        [HttpPost]
        public HttpResponseMessage InsertCustomer([FromBody] CreateCustomerRest createCustomerRest)
        {
            try
            {
                var connection = new NpgsqlConnection(connectionString);
                var command = new NpgsqlCommand("INSERT INTO Customer (Id, FirstName, LastName) VALUES (@id, @firstName, @lastName)", connection);
                using (connection)
                  {
                    connection.Open();
                    Customer customer = new Customer(Guid.NewGuid(), createCustomerRest.FirstName, createCustomerRest.LastName);
                    
                    command.Parameters.AddWithValue("@id", customer.Id);
                    command.Parameters.AddWithValue("@firstName", customer.FirstName);
                    command.Parameters.AddWithValue("@lastName", customer.LastName);
                    int affectedRows = command.ExecuteNonQuery();
                    if(affectedRows > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, customer);
                    }
                  }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
            catch 
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Customer Didnt Create");

            }
        }

        [HttpDelete]
        [Route("api/customer/{customerId}")]
        public HttpResponseMessage DeleteCustomer(Guid customerId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string checkSql = "SELECT COUNT(*) FROM Customer WHERE Id = @id";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("id", customerId);
                        int customerCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (customerCount == 0)
                        {
                            var response = Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found");
                            return response;
                        }
                    }
                    string deleteSql = "DELETE FROM Customer WHERE Id = @id";
                    using (NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("id", customerId);
                        deleteCommand.Connection = connection;
                        int rowsAffected = deleteCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Customer deleted");
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to delete customer");
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
        [Route("api/customer/{Id}")]
        public HttpResponseMessage Update(Guid id, [FromBody] UpdateCustomerRest updateCustomerRest)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string checkSql = "SELECT COUNT(*) FROM Customer WHERE Id = @id";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("id", id);
                        int customerCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (customerCount == 0)
                        {
                            var response = Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found");
                            return response;
                        }
                    }

                    string updateSql = "UPDATE Customer SET FirstName = @firstName, LastName = @lastName WHERE Id = @id";
                    using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("id", id);
                        updateCommand.Parameters.AddWithValue("firstName", updateCustomerRest.FirstName);
                        updateCommand.Parameters.AddWithValue("lastName", updateCustomerRest.LastName);
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Customer updated");
                        }
                        else
                        { 
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to update customer");
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
        [Route("api/customer/{Id}")]
        public HttpResponseMessage GetCustomer(Guid Id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT Id, FirstName, LastName FROM Customer WHERE Id = @id";
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

                                Customer customer = new Customer(id, firstName, lastName);

                                var response = Request.CreateResponse(HttpStatusCode.OK, customer);
                                return response;
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetAllCustomers()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT Id, FirstName, LastName FROM Customer";
                    using (NpgsqlCommand command = new NpgsqlCommand(selectSql, connection))
                    {
                        List<Customer> customers = new List<Customer>();
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);
                                string firstName = reader.GetString(1);
                                string lastName = reader.GetString(2);
                                Customer customer = new Customer(id, firstName, lastName);
                                customers.Add(customer);
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, customers);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}