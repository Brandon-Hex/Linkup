using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserService.DTO;
using UserService.Entities;
using UserService.Persistence;

namespace UserService.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMapper _mapper;

    public UserController(ILogger<UserController> logger, AppDbContext dbContext, IMapper mapper)
    {
        _logger = logger;
        _appDbContext = dbContext;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDto)
    {
        if (createUserDto == null)
        {
            return BadRequest("No User specified.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var mappedEntity = _mapper.Map<UserEntity>(createUserDto);

            _appDbContext.Users.Add(mappedEntity);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtRoute("GetUserById", new { id = mappedEntity.Id }, mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<ActionResult<UserEntity>> GetUserById(int id)
    {
        var user = await _appDbContext.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult> GetAllUsers()
    {
        try
        {
            return Ok(await _appDbContext.Users.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }   
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        try
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23503")
        {
            // Handle foreign key constraint violation
            _logger.LogError("Foreign key constraint violation: {Message}", postgresEx.Message);
            return BadRequest("The specified user has orders. Please delete all linked orders before attempting to delete the user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
    {
        if (updateUserDto == null)
        {
            return BadRequest("No User specified.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingUser = await _appDbContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            _mapper.Map(updateUserDto, existingUser);

            _appDbContext.Users.Update(existingUser);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }
}
