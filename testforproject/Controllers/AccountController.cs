using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

public class AccountController : Controller

{
    private readonly IJwtService _jwtService;
    public AccountController(IJwtService jwtService) {
        _jwtService = jwtService;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        if (_jwtService.UserId != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(); 
    }
    [HttpGet]
    public IActionResult Login()
    {
        if (_jwtService.UserId != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }


}