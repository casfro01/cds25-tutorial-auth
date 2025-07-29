using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

public class Pbkdf2PasswordHasher : PasswordHasher<User> { }