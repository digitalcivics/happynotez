using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HappyNotez.Models;
using Microsoft.AspNet.Mvc;

namespace HappyNotez.Controllers
{
    public class NotezController : Controller
    {
        static XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());

        static XNamespace atom = "http://www.w3.org/2005/Atom";
        static string atomUpdatedFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        static NotezController()
        {
            nsManager.AddNamespace("atom", atom.NamespaceName);
        }

        private NotezContext _context;

        public NotezController(NotezContext context)
        {
            _context = context;
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Coords()
        {
            List<Notez> notez = (from n in _context.Notez
                                 where n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid
                                 select n).ToList();

            return Json(new
            {
                count = notez.Count,
                ids = notez.Select(n => n.ID).ToArray(),
                founds = notez.Select(n => (n.Timestamp.Year - 2015) * 32 * 12 + n.Timestamp.Month * 32 + n.Timestamp.Day).ToArray(),
                lats = notez.Select(n => (float)n.Latitude).ToArray(),
                longs = notez.Select(n => (float)n.Longitude).ToArray(),
            });
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Ids()
        {
            List<Notez> notez = (from n in _context.Notez
                                 where n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid
                                 orderby n.Timestamp descending
                                 select n).ToList();

            return Json(new
            {
                count = notez.Count,
                ids = notez.Select(n => n.ID).ToArray(),
                flags = notez.Select(n => n.FlagStatus).ToArray()
            });
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Report(int id)
        {
            Notez note = _context.Notez.Where(n => n.ID == id).FirstOrDefault();

            if (note == null)
                return Content("Don't worry, be happy!");

            if (note.ReportCount < int.MaxValue)
                note.ReportCount++;

            try
            {
                if (note.FlagStatus == FlagStatus.Approved)
                    return Content("This note has been reported previously, reviewed and approved.");
             
                if (note.FlagStatus == FlagStatus.NotFlaggged)
                    note.FlagStatus = FlagStatus.Flagged;

                return Content("Thank you for keeping the community happy.");
            }
            finally
            {
                _context.SaveChanges();
            }
        }

        public IActionResult HappyApprove(int id)
        {
            Notez note = _context.Notez.Where(n => n.ID == id).FirstOrDefault();

            if (note != null && note.FlagStatus == FlagStatus.Flagged)
            {
                note.FlagStatus = FlagStatus.Approved;
                _context.SaveChanges();
            }

            return RedirectToAction("FlaggedFeed");
        }

        public IActionResult HappyRemove(int id)
        {
            Notez note = _context.Notez.Where(n => n.ID == id).FirstOrDefault();

            if (note != null && note.FlagStatus == FlagStatus.Flagged)
            {
                note.FlagStatus = FlagStatus.Removed;
                _context.SaveChanges();
            }

            return RedirectToAction("FlaggedFeed");
        }

        private XDocument GenerateFeed(IEnumerable<Notez> notes, string title, string noteTitle, Func<Notez, string> html)
        {
            XElement outputFeed = new XElement(atom + "feed");
            XDocument xml = new XDocument(outputFeed);

            outputFeed.AddFirst(
                        new XElement(atom + "title", title),
                        new XElement(atom + "subtitle", "Why don't you leave a happy note for someone?"),
                        new XElement(atom + "link", new XAttribute("href", "http://happynotez.org/"))
                    );

            bool first = true;

            foreach (Notez note in notes)
            {
                if (first)
                {
                    outputFeed.Add(new XElement(atom + "updated", note.Timestamp.ToString(atomUpdatedFormat)));
                    first = false;
                }

                outputFeed.Add(
                    new XElement(atom + "entry",
                        new XElement(atom + "title", noteTitle),
                        new XElement(atom + "id", note.ID),
                        new XElement(atom + "updated", note.Timestamp.ToString(atomUpdatedFormat)),
                        new XElement(atom + "link", new XAttribute("href", "http://happynotez.org/" + note.ID)),
                        new XElement(atom + "content",
                            new XAttribute("type", "html"), html(note))
                        ));
            }

            return xml;
        }

        [ResponseCache(Duration = 15 * 60)]
        public IActionResult Feed()
        {
            XDocument xml = GenerateFeed(_context.Notez.Where(n => n.FlagStatus != FlagStatus.Removed && n.FlagStatus != FlagStatus.Invalid).OrderByDescending(n => n.ID).Take(20), "HappyNotez.org", "New note has been found!", n => $"<a href='/{n.ID}'><img src='/n/t{n.ID}.jpg' /></a>");

            return Content(xml.ToString(), "application/atom+xml");
        }

        [ResponseCache(Duration = 15 * 60)]
        public IActionResult FlaggedFeed()
        {
            XDocument xml = GenerateFeed(_context.Notez.Where(n => n.FlagStatus == FlagStatus.Flagged), "Flagged HappyNotez.org", "A note has been flagged", n => $"<img src='/n/t{n.ID}.jpg' /><br/>Flagged count: {n.ReportCount} <a href='/notez/happyapprove/{n.ID}'>Approve</a> <a href='/notez/happyremove/{n.ID}'>Remove</a>");

            return Content(xml.ToString(), "application/atom+xml");
        }
    }
}
