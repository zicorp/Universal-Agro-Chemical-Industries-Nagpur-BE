﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using static Dugros_Api.Controllers.ItemCategoryController;
using Microsoft.AspNetCore.Authorization;

namespace Dugros_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UOMController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UOMController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class GetUOM
        {
            public Guid uom_id { get; set; }
            public string uom_name { get; set; }
            public string uom_abbreviation { get; set; }
            public int is_active { get; set; }
        }

        public class AddUOM
        {
            public Guid user_id { get; set; }
            public string uom_name { get; set; }
            public string uom_abbreviation { get; set; }
        }

        public class EditUOMModel
        {
            public Guid user_id { get; set; }
            public string uom_name { get; set; }
            public string uom_abbreviation { get; set; }
            public int is_active { get; set; }
        }

        public class DeleteUOMModel
        {
            public Guid user_id { get; set; }
        }

        [HttpGet]
        public IActionResult GetUOMS(Guid userId)
        {
            try
            {
                List<GetUOM> itemCategories = new List<GetUOM>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.UOMGet", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@user_id", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    GetUOM itemCategory = new GetUOM
                                    {
                                        uom_id = (Guid)reader["uom_id"],
                                        uom_name = reader["uom_name"].ToString(),
                                        uom_abbreviation = reader["uom_abbreviation"].ToString(),
                                        is_active = Convert.ToInt32(reader["is_active"])
                                    };

                                    itemCategories.Add(itemCategory);
                                }
                            }
                        }
                    }
                }

                if (itemCategories.Any())
                {
                    return Ok(itemCategories);
                }
                else
                {
                    return NotFound("No item categories found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("addUOM")]
        public IActionResult PostUOM(AddUOM addUOM)
        {
            try
            {
                string message;
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.UOMInsert", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@user_id", addUOM.user_id);
                        command.Parameters.AddWithValue("@uom_name", addUOM.uom_name);
                        command.Parameters.AddWithValue("@uom_abbreviation", addUOM.uom_abbreviation);

                        // Add OUTPUT parameter to capture the stored procedure message
                        var outputParam = new SqlParameter("@Message", SqlDbType.NVarChar, 1000);
                        outputParam.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outputParam);

                        // Execute the stored procedure
                        command.ExecuteNonQuery();

                        // Get the message from the output parameter
                        message = command.Parameters["@Message"].Value.ToString();
                    }
                }

                // Check the message returned by the stored procedure
                if (message.StartsWith("UOM inserted successfully"))
                {
                    return Ok(new { ExecuteMessage = message });
                }
                else
                {
                    return BadRequest(message); // Return error message
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("edit/{uom_id}")]
        public IActionResult EditItemCategory(Guid uom_id, [FromBody] EditUOMModel editUOM)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.UOMEdit", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@user_id", editUOM.user_id);
                        command.Parameters.AddWithValue("@uom_id", uom_id);
                        command.Parameters.AddWithValue("@uom_name", editUOM.uom_name);
                        command.Parameters.AddWithValue("@uom_abbreviation", editUOM.uom_abbreviation);
                        command.Parameters.AddWithValue("@is_active", editUOM.is_active);

                        // Execute the stored procedure
                        var successMessageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(successMessageParam);
                        command.Parameters.Add(errorMessageParam);

                        command.ExecuteNonQuery();

                        string successMessage = successMessageParam.Value?.ToString();
                        string errorMessage = errorMessageParam.Value?.ToString();

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            return StatusCode(400, errorMessage); // Return bad request with error message
                        }
                        else if (!string.IsNullOrEmpty(successMessage))
                        {
                            return Ok(new { ExecuteMessage = successMessage }); // Return success message
                        }
                        else
                        {
                            return StatusCode(500, "Error: No response from the database."); // No response from database
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}"); // Internal server error
            }
        }

        [HttpPut("delete/{uom_id}")]
        public IActionResult DeleteItemCategory(Guid uom_id, [FromBody] DeleteUOMModel deleteUOMModel)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.UOMDelete", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@user_id", deleteUOMModel.user_id);
                        command.Parameters.AddWithValue("@uom_id", uom_id);

                        // Define output parameters for messages
                        var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(messageParam);
                        command.Parameters.Add(errorMessageParam);

                        command.ExecuteNonQuery();

                        string message = messageParam.Value?.ToString();
                        string errorMessage = errorMessageParam.Value?.ToString();

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            return StatusCode(400, errorMessage); // Return bad request with error message
                        }
                        else if (!string.IsNullOrEmpty(message))
                        {
                            return Ok(new { ExecuteMessage = message }); // Return success message
                        }
                        else
                        {
                            return StatusCode(500, "Error: No response from the database."); // No response from database
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}"); // Internal server error
            }
        }

    }
}