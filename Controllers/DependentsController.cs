using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DependentsController : ControllerBase
    {
        private readonly BenefitsDataContext _context;

        public DependentsController(BenefitsDataContext context)
        {
            _context = context;
        }

        // GET: api/Dependents
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Dependent>>> GetEmployeeDependents(int employeeId)
        {
            return await _context.Dependents.Where(d=>d.EmployeeId == employeeId).ToListAsync();
        }

        // GET: api/Dependents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dependent>> GetDependent(int id)
        {
            var dependent = await _context.Dependents.FindAsync(id);

            if (dependent == null)
            {
                return NotFound();
            }

            return dependent;
        }

        // PUT: api/Dependents/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDependent(int id, Dependent dependent)
        {
            if (id != dependent.Id)
            {
                return BadRequest();
            }

            _context.Entry(dependent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DependentExists(id))
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

        // POST: api/Dependents
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<Dependent> PostDependent(Dependent dependent)
        {
            _context.Dependents.Add(dependent);
            await _context.SaveChangesAsync();

            return dependent;
        }

        // DELETE: api/Dependents/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Dependent>> DeleteDependent(int id)
        {
            var dependent = await _context.Dependents.FindAsync(id);
            if (dependent == null)
            {
                return NotFound();
            }

            _context.Dependents.Remove(dependent);
            await _context.SaveChangesAsync();

            return dependent;
        }

        private bool DependentExists(int id)
        {
            return _context.Dependents.Any(e => e.Id == id);
        }
    }
}
