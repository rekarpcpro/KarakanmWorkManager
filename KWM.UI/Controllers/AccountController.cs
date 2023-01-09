using KWM.Application.Models;
using KWM.Application.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace KWM.UI.Controllers;
[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<AppUserModel> _signInManager;
    private readonly UserManager<AppUserModel> _userManager;

    public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager)
    {
        
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("Login")]
    public IActionResult Login(string returnUrl)
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["returnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null)
        {
            ModelState.AddModelError("", "Email not found!");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.StayLoggedIn, false);

        if (result.Succeeded)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) == false && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("account/logout", StringComparison.OrdinalIgnoreCase) == false)
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Email and password are mismatch!");
        return View(model);
    }

    [HttpGet("Register")]
    public IActionResult Register(string returnUrl)
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["returnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new AppUserModel();
        user.UserName = model.Email;
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var createResult = await _userManager.CreateAsync(user, model.Password);

        if (createResult.Succeeded == false)
        {
            foreach (var e in createResult.Errors)
            {
                ModelState.AddModelError(e.Code, e.Description);
            }
            return View(model);
        }

        await _signInManager.SignInAsync(user, model.StayLoggedIn);
        //todo - buy a domain and setup email service - await _emailService.SendEmail(model.Email, "Registration", "Welcome to my blog.");

        if (string.IsNullOrWhiteSpace(returnUrl) == false && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction("index", "Panel");
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet("LogOut")]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("GoogleLogin")]
    public IActionResult GoogleLogin()
    {
        string redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return new ChallengeResult("Google", properties);
    }

    [HttpGet("GoogleResponse")]
    public async Task<IActionResult> GoogleResponse()
    {
        ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Task");
        }
        else
        {
            string email = info.Principal.FindFirst(ClaimTypes.Email).Value;

            var u = await _userManager.FindByEmailAsync(email);

            if (u != null)
            {
                await _signInManager.SignInAsync(u, true);
                return RedirectToAction("Index", "Task");
            }

            AppUserModel user = new AppUserModel
            {
                Email = email,
                UserName = email,
            };

            IdentityResult identResult = await _userManager.CreateAsync(user);

            if (identResult.Succeeded)
            {
                identResult = await _userManager.AddLoginAsync(user, info);

                if (identResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, true);
                    return RedirectToAction("Index", "Task");
                }
            }
            return RedirectToAction("Forbidden");
        }
    }

    [HttpGet("Forbidden")]
    public async Task<IActionResult> Forbidden()
    {
        return View();
    }

}
