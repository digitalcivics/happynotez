﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HappyNotez.Models;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;

namespace HappyNotez.Controllers
{
    public class HomeController : Controller
    {
        private NotezContext _context;
        private IHostingEnvironment _environment;

        public HomeController(NotezContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index(long? id)
        {            
            return View(_context.Notez.Where(n => n.ID == id && n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid).FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile photo, string query, string latitude, string longitude, string elevation, string zoom, bool found)
        {
            if (photo != null)
            {
                Notez note = new Notez
                {
                    Found = found,
                    Timestamp = DateTimeOffset.Now,
                    UserAgent = Request.Headers["User-Agent"],
                    HostAddress = Context.GetFeature<IHttpConnectionFeature>().RemoteIpAddress.ToString(),
                    LocationRaw = query,
                    Latitude = double.Parse(latitude, CultureInfo.InvariantCulture),
                    Longitude = double.Parse(longitude, CultureInfo.InvariantCulture),
                    Zoom = float.Parse(zoom, CultureInfo.InvariantCulture),
                    Elevation = float.Parse(elevation, CultureInfo.InvariantCulture),
                };

                _context.Notez.Add(note);
                await _context.SaveChangesAsync();

                string root = Path.Combine(_environment.MapPath("n"));
                await photo.SaveAsAsync(Path.Combine(root, note.ID + ".jpg"));

                try
                {
                    using (Stream s = photo.OpenReadStream())
                        Helper.GenerateThumbnail(s, Path.Combine(root, "t" + note.ID + ".jpg"));
                }
                catch
                {
                    note.FlagStatus = FlagStatus.Invalid;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index), new { noteID = note.ID });
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
