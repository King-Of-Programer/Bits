using ContactManager.Data;
using ContactManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace ContactManager.Controllers
{
  
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts.ToListAsync();
            return View(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid CSV file.");
                Console.WriteLine("No file uploaded.");
                return RedirectToAction("Index");
            }

            try
            {
                
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    bool isFirstLine = true;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        Console.WriteLine($"Reading line: {line}");

                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; 
                        }

                        var values = line.Split(',');
                        Console.WriteLine($"Split values: {string.Join(",", values)}"); 

                        
                        if (values.Length == 6)
                        {
                            try
                            {
                                var contact = new Contact
                                {
                                    
                                    Name = values[1],
                                    DateOfBirth = DateTime.ParseExact(values[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    Married = bool.Parse(values[3]),
                                    Phone = values[4],
                                    Salary = decimal.Parse(values[5], CultureInfo.InvariantCulture) 
                                };

                             
                                _context.Contacts.Add(contact);
                                Console.WriteLine($"Adding contact: {contact.Name}, {contact.DateOfBirth}, {contact.Married}, {contact.Phone}, {contact.Salary}");
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine($"Error processing line: {line}, Exception: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipping invalid line: {line}"); 
                        }
                    }

                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine("CSV data saved to database.");
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error opening or processing the file: {ex.Message}");
            }

            return RedirectToAction("Index");
        }




        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] Contact updatedContact)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            contact.Name = updatedContact.Name;
            contact.DateOfBirth = updatedContact.DateOfBirth;
            contact.Married = updatedContact.Married;
            contact.Phone = updatedContact.Phone;
            contact.Salary = updatedContact.Salary;

            await _context.SaveChangesAsync();
            return Ok(); 
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return Ok(); 
        }



    }
}
