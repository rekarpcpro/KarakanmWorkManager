using KWM.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWM.Application.Data;
public class AppDbContext : IdentityDbContext<AppUserModel>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
		
	}

	public DbSet<MainTaskModel> Task { get; set; }
	public DbSet<SubTaskModel> SubTask { get; set; }
}
