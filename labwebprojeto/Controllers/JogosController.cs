﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using labwebprojeto.Data;
using labwebprojeto.Models;
using labwebprojeto.Services.Interfaces;
using labwebprojeto.ViewModels;
using System.Security.Claims;

namespace labwebprojeto.Controllers
{
    public class JogosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoService _photoService;
        private readonly IEmailService _emailService;

        public JogosController(ApplicationDbContext context, 
            IPhotoService photoService, 
            IEmailService emailService)
        {
            _context = context;
            _photoService = photoService;
            _emailService = emailService;
        }

        // GET: Jogos
        public async Task<IActionResult> Index(string searchString)
        {
            var jogos = from j in _context.Jogos
                        select j;

            if (!String.IsNullOrEmpty(searchString))
            {
                jogos = jogos.Where(j => j.Nome!.Contains(searchString));
            }

            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nome");
            ViewData["IdConsola"] = new SelectList(_context.Consolas, "IdConsola", "Nome");
            ViewData["IdProdutora"] = new SelectList(_context.Produtoras, "IdProdutora", "Nome");

            var applicationDbContext = _context.Jogos.
                Include(j => j.IdCategoriaNavigation)
                .Include(j => j.IdConsolaNavigation)
                .Include(j => j.IdProdutoraNavigation);
            return View(await jogos.ToListAsync());
        }

        // GET: Jogos/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var categoria = (from j in _context.Jogos
                             join c in _context.Categoria on j.IdCategoria equals c.IdCategoria
                             select c.Nome).Distinct().ToList();
            ViewData["Categoria"] = categoria;

            var consola = (from j in _context.Jogos
                           join c in _context.Consolas on j.IdCategoria equals c.IdConsola
                           select c.Nome).Distinct().ToList();
            ViewData["Consola"] = consola;

            var produtora = (from j in _context.Jogos
                             join c in _context.Produtoras on j.IdCategoria equals c.IdProdutora
                             select c.Nome).Distinct().ToList();
            ViewData["Produtora"] = produtora;



            if (id == null || _context.Jogos == null)
            {
                return NotFound();
            }

            var jogo = await _context.Jogos
                .Include(j => j.IdCategoriaNavigation)
                .Include(j => j.IdConsolaNavigation)
                .Include(j => j.IdProdutoraNavigation)
                .FirstOrDefaultAsync(m => m.IdJogos == id);
            if (jogo == null)
            {
                return NotFound();
            }

            return View(jogo);
        }

        // GET: Jogos/Create
        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nome");
            ViewData["IdConsola"] = new SelectList(_context.Consolas, "IdConsola", "Nome");
            ViewData["IdProdutora"] = new SelectList(_context.Produtoras, "IdProdutora", "Nome");
            return View();
        }

        // POST: Jogos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateJogoViewModel jogoVM)
        {
            if (ModelState.IsValid)
            {
                var result_pic = await _photoService.AddPhotoAsync(jogoVM.Foto);
                var result_pic1 = await _photoService.AddPhotoAsync(jogoVM.Foto1);
                var result_pic2 = await _photoService.AddPhotoAsync(jogoVM.Foto2);
                var jogo = new Jogo
                {
                    IdJogos = jogoVM.IdJogos,
                    Nome = jogoVM.Nome,
                    Foto = result_pic.Url.ToString(),
                    Foto1 = result_pic1.Url.ToString(),
                    Foto2 = result_pic2.Url.ToString(),
                    IdCategoria = jogoVM.IdCategoria,
                    IdConsola = jogoVM.IdConsola,
                    IdProdutora = jogoVM.IdProdutora,
                    Preco = Math.Round(jogoVM.Preco, 2),
                    Descricao = jogoVM.Descricao,
                    Descricao1 = jogoVM.Descricao1,
                };
                _context.Add(jogo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Game Created Successfully";

                //Add Email Notification
                var userClients = (from u in _context.Utilizadors
                                 select u)
                                 .Where(x => x.IsCliente == true);

                //Lista de emails
                var emailClients = (from u in userClients
                                    select u.Email);

                //favoritos que tem um cliente associado
                var categoryClients = (from f in _context.Favoritos
                                       join u in userClients
                                       on f.IdUtilizador equals u.IdUtilizador
                                       select f);

                var favourites = categoryClients.Where(x => x.IdCategoria == jogoVM.IdCategoria);

                if(favourites.Any())
                {
                    foreach (var c in emailClients)
                    {
                        await _emailService.SendEmailAsync(c, "New Game - " + jogoVM.Nome, "New game added!");
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Game Created Successfully";
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nome", jogoVM.IdCategoria);
            ViewData["IdConsola"] = new SelectList(_context.Consolas, "IdConsola", "Nome", jogoVM.IdConsola);
            ViewData["IdProdutora"] = new SelectList(_context.Produtoras, "IdProdutora", "Nome", jogoVM.IdProdutora);

            return View(jogoVM);
        }

        // GET: Jogos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Jogos == null)
            {
                return NotFound();
            }

            var jogo = await _context.Jogos.FindAsync(id);
            if (jogo == null)
            {
                return NotFound();
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nome", jogo.IdCategoria);
            ViewData["IdConsola"] = new SelectList(_context.Consolas, "IdConsola", "Nome", jogo.IdConsola);
            ViewData["IdProdutora"] = new SelectList(_context.Produtoras, "IdProdutora", "Nome", jogo.IdProdutora);
            return View(jogo);
        }

        // POST: Jogos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("IdJogos," +
            "Nome,Foto,Foto1," +
            "Foto2,IdCategoria," +
            "IdConsola,IdProdutora," +
            "Preco,Descricao,Descricao1")] Jogo jogo)
        {
            if (id != jogo.IdJogos)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jogo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JogoExists(jogo.IdJogos))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nome", jogo.IdCategoria);
            ViewData["IdConsola"] = new SelectList(_context.Consolas, "IdConsola", "Nome", jogo.IdConsola);
            ViewData["IdProdutora"] = new SelectList(_context.Produtoras, "IdProdutora", "Nome", jogo.IdProdutora);
            return View(jogo);
        }

        // GET: Jogos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Jogos == null)
            {
                return NotFound();
            }

            var jogo = await _context.Jogos
                .Include(j => j.IdCategoriaNavigation)
                .Include(j => j.IdConsolaNavigation)
                .Include(j => j.IdProdutoraNavigation)
                .FirstOrDefaultAsync(m => m.IdJogos == id);
            if (jogo == null)
            {
                return NotFound();
            }

            return View(jogo);
        }

        // POST: Jogos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Jogos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Jogos'  is null.");
            }
            var jogo = await _context.Jogos.FindAsync(id);
            if (jogo != null)
            {
                _context.Jogos.Remove(jogo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JogoExists(int id)
        {
          return _context.Jogos.Any(e => e.IdJogos == id);
        }


        /*--------CATEGORIAS*---------*/
        public async Task<IActionResult> IndexCategorias()
        {
            var categorias = (from c in _context.Categoria
                              join j in _context.Jogos
                              on c.IdCategoria equals j.IdCategoria
                              select c).Distinct();

            return View(await categorias.ToListAsync());
        }

        public async Task<IActionResult> DetailsCategorias(int id)
        {
            var jogo = (from j in _context.Jogos
                        join c in _context.Categoria
                        on j.IdCategoria equals id
                        select j).Distinct();

            return View(await jogo.ToListAsync());
        }

        /*--------PRODUTORAS*---------*/
        public async Task<IActionResult> IndexProdutoras()
        {
            var produtoras = (from p in _context.Produtoras
                              join j in _context.Jogos
                              on p.IdProdutora equals j.IdProdutora
                              select p).Distinct();

            return View(await produtoras.ToListAsync());
        }

        public async Task<IActionResult> DetailsProdutoras(int id)
        {
            var jogo = (from j in _context.Jogos
                        join p in _context.Produtoras
                        on j.IdCategoria equals id
                        select j).Distinct();

            return View(await jogo.ToListAsync());
        }

        /*--------CONSOLAS*---------*/
        public async Task<IActionResult> IndexConsolas()
        {
            var consolas = (from c in _context.Consolas
                              join j in _context.Jogos
                              on c.IdConsola equals j.IdCategoria
                              select c).Distinct();

            return View(await consolas.ToListAsync());
        }

        public async Task<IActionResult> DetailsConsolas(int id)
        {
            var jogo = (from j in _context.Jogos
                        join c in _context.Categoria
                        on j.IdCategoria equals id
                        select j).Distinct();

            return View(await jogo.ToListAsync());
        }
    }



}
