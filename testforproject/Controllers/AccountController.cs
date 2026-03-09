using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

public class AccountController : Controller

{
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _db;
    public AccountController(IJwtService jwtService, ApplicationDbContext db)
    {
        _jwtService = jwtService;
        _db = db;
    }

    [HttpGet]
    public IActionResult Onboarding()
    {
        if (_jwtService.UserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var categories = _db.Categories.ToList();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_jwtService.UserId != null)
        {
            return RedirectToAction("Show", "Dashboard");
        }
        return View();
    }
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (_jwtService.UserId != null)
        {
            return RedirectToAction("Show", "Dashboard");
        }
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return RedirectToAction("Show", "Dashboard");
    }
}