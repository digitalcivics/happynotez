using System;
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
    public class MobileController : Controller
    {
        private const int PerPage = 5;

        private NotezContext _context;
        private IHostingEnvironment _environment;

        public MobileController(NotezContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Thanks(long noteID)
        {
            return View(noteID);
        }

        public IActionResult Archive(int p = 0)
        {
            ViewBag.Page = p;
            ViewBag.PerPage = PerPage;

            Notez[] notes = _context.Notez.Where(n => n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid).OrderByDescending(n => n.ID).Skip(p * PerPage).Take(PerPage).ToArray();
            if (notes.Length == 0 && p != 0)
                return RedirectToAction(nameof(Archive)); 

            return View(notes);
        }

        public IActionResult Item(long noteID)
        {
            Notez note = _context.Notez.Where(n => n.ID == noteID && n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid).FirstOrDefault();

            if (note == null)
                return RedirectToAction(nameof(Index));

            return View(note);
        }

        public IActionResult Index()
        {
            return View();
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

                return RedirectToAction("Index", new { noteID = note.ID });
            }

            return RedirectToAction("Index");
        }
    }
}
