using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Services;

namespace ServerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaychecksController : ControllerBase
    {
        private readonly BenefitsDataContext _context;
        private readonly IPaycheckService _paycheckService;

        public PaychecksController(BenefitsDataContext context, IPaycheckService paycheckService)
        {
            _context = context;
            _paycheckService = paycheckService;
        }

        // GET: api/Paychecks
        [HttpGet("ProcessPaychecks")]
        public async Task<ActionResult> ProcessPaychecks(int year, int employeeId)
        {
            await _paycheckService.ProcessPaychecks(year, employeeId);
            return NoContent();
        }
        // GET: api/Paychecks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paycheck>>> GetEmployeePaychecks(int employeeId, int year)
        {
            return await _context.Paychecks.Where(p=>p.EmployeeId == employeeId && p.Year == year).OrderBy(p=>p.Index).ToListAsync();
        }
        [HttpGet("GetEmployeePaycheckYears")]
        public async Task<ActionResult<IEnumerable<int>>> GetEmployeePaycheckYears(int employeeId)
        {
            return await _context.Paychecks.Where(p => p.EmployeeId == employeeId).Select(p=>p.Year).Distinct().OrderByDescending(y=>y).ToListAsync();
        }
        // GET: api/Paychecks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Paycheck>> GetPaycheck(int id)
        {
            var paycheck = await _context.Paychecks.FindAsync(id);

            if (paycheck == null)
            {
                return NotFound();
            }

            return paycheck;
        }

        // PUT: api/Paychecks/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaycheck(int id, Paycheck paycheck)
        {
            if (id != paycheck.Id)
            {
                return BadRequest();
            }

            _context.Entry(paycheck).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaycheckExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Paychecks
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Paycheck>> PostPaycheck(Paycheck paycheck)
        {
            _context.Paychecks.Add(paycheck);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaycheck", new { id = paycheck.Id }, paycheck);
        }

        // DELETE: api/Paychecks/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Paycheck>> DeletePaycheck(int id)
        {
            var paycheck = await _context.Paychecks.FindAsync(id);
            if (paycheck == null)
            {
                return NotFound();
            }

            _context.Paychecks.Remove(paycheck);
            await _context.SaveChangesAsync();

            return paycheck;
        }

        private bool PaycheckExists(int id)
        {
            return _context.Paychecks.Any(e => e.Id == id);
        }
    }
}
