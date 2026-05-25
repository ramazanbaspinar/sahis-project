using Microsoft.EntityFrameworkCore;
using Orion.Data.Context;
using Orion.Data.Entities;
using Orion.Models;

namespace Orion.Services
{
    public class SahisService
    {
        private readonly IDbContextFactory<SahisDbContext> _contextFactory;
        private readonly ILogger<SahisService> _logger;

        public SahisService(IDbContextFactory<SahisDbContext> contextFactory, ILogger<SahisService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<(List<PersonDto> Data, int TotalCount)> SearchAsync(
            string? tc,
            string? ad, string? soyad,
            string? il, string? ilce,
            string? anne, string? baba,
            string? anneTc, string? babaTc,
            string? gsm,
            string? cinsiyet,
            int? minYas, int? maxYas,
            SearchMode searchMode = SearchMode.Contains,
            int page = 1, int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var query = context.Citizens.AsNoTracking().AsQueryable();
            bool isFiltered = false;

            _logger.LogInformation("Yeni Sorgu Yapıldı: TC={Tc}, Ad={Ad}, Soyad={Soyad}, İl={Il}, İlçe={Ilce}", tc, ad, soyad, il, ilce);

            string Normalize(string s) => s.ToLower(new System.Globalization.CultureInfo("tr-TR"))
                .Replace('ğ', 'g').Replace('ü', 'u').Replace('ş', 's')
                .Replace('ı', 'i').Replace('ö', 'o').Replace('ç', 'c');

            // 1. TC KİMLİK (Kişinin Kendisi)
            if (!string.IsNullOrWhiteSpace(tc) && long.TryParse(tc, out long tckn))
            {
                query = query.Where(p => p.Tc == tckn);
                return await ExecuteQuery(query, page, pageSize, cancellationToken);
            }

            // --- ANNE TC ---
            if (!string.IsNullOrWhiteSpace(anneTc) && long.TryParse(anneTc, out long atc))
            {
                query = query.Where(p => p.AnneTc == atc);
                isFiltered = true;
            }

            // --- BABA TC ---
            if (!string.IsNullOrWhiteSpace(babaTc) && long.TryParse(babaTc, out long btc))
            {
                query = query.Where(p => p.BabaTc == btc);
                isFiltered = true;
            }

            // 2. GSM
            if (!string.IsNullOrWhiteSpace(gsm))
            {
                string cleanGsm = gsm.Replace(" ", "").Trim();
                query = query.Where(p => EF.Functions.Like(p.GsmListesi, $"%{cleanGsm}%"));
                isFiltered = true;
            }

            // 3. İL VE İLÇE
            if (!string.IsNullOrWhiteSpace(il))
            {
                string nIl = Normalize(il);
                query = query.Where(p => context.TrToEn(p.NufusIl) == nIl);
                isFiltered = true;
            }
            if (!string.IsNullOrWhiteSpace(ilce))
            {
                string nIlce = Normalize(ilce);
                query = query.Where(p => context.TrToEn(p.NufusIlce) == nIlce);
                isFiltered = true;
            }

            // 4. AD VE SOYAD
            bool adVar = !string.IsNullOrWhiteSpace(ad);
            bool soyadVar = !string.IsNullOrWhiteSpace(soyad);

            if (adVar || soyadVar)
            {
                string nAd = adVar ? Normalize(ad!) : "";
                string nSoyad = soyadVar ? Normalize(soyad!) : "";

                if (searchMode == SearchMode.Exact)
                {
                    if (adVar) query = query.Where(p => context.TrToEn(p.Ad) == nAd);
                    if (soyadVar) query = query.Where(p => context.TrToEn(p.Soyad) == nSoyad);
                }
                else
                {
                    if (adVar) query = query.Where(p => context.TrToEn(p.Ad).Contains(nAd));
                    if (soyadVar) query = query.Where(p => context.TrToEn(p.Soyad).Contains(nSoyad));
                }
                isFiltered = true;
            }

            // 5. DİĞER FİLTRELER
            if (!string.IsNullOrWhiteSpace(anne))
            {
                query = query.Where(p => EF.Functions.ILike(p.AnneAdi, $"{anne}%"));
                isFiltered = true;
            }
            if (!string.IsNullOrWhiteSpace(baba))
            {
                query = query.Where(p => EF.Functions.ILike(p.BabaAdi, $"{baba}%"));
                isFiltered = true;
            }
            if (!string.IsNullOrWhiteSpace(cinsiyet) && cinsiyet != "Hepsi")
            {
                query = query.Where(p => EF.Functions.ILike(p.Cinsiyet, $"{cinsiyet}%"));
                isFiltered = true;
            }

            // Yaş Filtresi
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (minYas.HasValue) query = query.Where(p => p.DogumTarihi <= today.AddYears(-minYas.Value));
            if (maxYas.HasValue) query = query.Where(p => p.DogumTarihi >= today.AddYears(-(maxYas.Value + 1)).AddDays(1));

            if (!isFiltered) return (new List<PersonDto>(), 0);

            return await ExecuteQuery(query, page, pageSize, cancellationToken);
        }

        private async Task<(List<PersonDto>, int)> ExecuteQuery(
            IQueryable<Citizen> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int maxLimit = 1000;
            var countQuery = query.Take(maxLimit + 1);
            var totalCount = await countQuery.CountAsync(cancellationToken);

            bool hasMore = false;
            if (totalCount > maxLimit)
            {
                hasMore = true;
                totalCount = maxLimit;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var result = await query
                .OrderBy(p => p.Ad).ThenBy(p => p.Soyad)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToListAsync(cancellationToken);

            return (result, hasMore ? maxLimit + 1 : totalCount);
        }

        public static readonly System.Linq.Expressions.Expression<Func<Citizen, PersonDto>> MapToDto = p => new PersonDto
        {
            Tc = p.Tc,
            FullName = $"{p.Ad} {p.Soyad}",

            AnneTc = p.AnneTc,
            BabaTc = p.BabaTc,
            OlumTarihi = p.OlumTarihi.HasValue ? p.OlumTarihi.Value.ToString("dd.MM.yyyy") : "-",
            NufusKoy = p.NufusKoy ?? "-",

            AnneBaba = $"{p.AnneAdi} / {p.BabaAdi}",
            DogumBilgisi = p.DogumTarihi.HasValue ? p.DogumTarihi.Value.ToString("dd.MM.yyyy") : "",
            DogumYeri = p.DogumYeri ?? "-",
            NufusIl = p.NufusIl ?? "-",
            NufusIlce = p.NufusIlce ?? "-",
            Lokasyon = $"{p.NufusIl} / {p.NufusIlce}",
            Cinsiyet = p.Cinsiyet,
            MedeniHal = p.MedeniHal == "EVLI" ? "EVLİ" :
                       (p.MedeniHal == "BOSANMIS" ? "BOŞANMIŞ" : p.MedeniHal),
            Adres = p.Ikametgah ?? "Adres Bilgisi Yok",
            Uyruk = (p.Uyruk == null || p.Uyruk == "[null]" || p.Uyruk == "\\N") ? "" : p.Uyruk,
            VergiNo = p.VergiNo ?? "-",
            Telefonlar = p.GsmListesi != null
                ? p.GsmListesi.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>()
        };

        public async Task<FamilyTreeDto?> GetFamilyTreeAsync(long tc, CancellationToken cancellationToken = default)
        {
            using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var query = context.Citizens.AsNoTracking();

            var targetPerson = await query.Where(p => p.Tc == tc).Select(MapToDto).FirstOrDefaultAsync(cancellationToken);
            if (targetPerson == null) return null;

            var tree = new FamilyTreeDto { Person = targetPerson };

            // 1. Ebeveynleri Bul (Anne / Baba)
            var parentTcs = new List<long>();
            if (targetPerson.AnneTc.HasValue && targetPerson.AnneTc > 0) parentTcs.Add(targetPerson.AnneTc.Value);
            if (targetPerson.BabaTc.HasValue && targetPerson.BabaTc > 0) parentTcs.Add(targetPerson.BabaTc.Value);

            if (parentTcs.Any())
            {
                tree.Parents = await query.Where(p => parentTcs.Contains(p.Tc)).Select(MapToDto).ToListAsync(cancellationToken);
            }

            // 2. Çocukları Bul (BabaTc == tc veya AnneTc == tc)
            tree.Children = await query.Where(p => p.BabaTc == tc || p.AnneTc == tc).Select(MapToDto).ToListAsync(cancellationToken);

            // 3. Kardeşleri Bul (Aynı Anne veya Aynı Baba, kendisi hariç)
            if (parentTcs.Any())
            {
                var anneTc = targetPerson.AnneTc;
                var babaTc = targetPerson.BabaTc;

                tree.Siblings = await query.Where(p => p.Tc != tc &&
                                           ((anneTc.HasValue && anneTc > 0 && p.AnneTc == anneTc) ||
                                            (babaTc.HasValue && babaTc > 0 && p.BabaTc == babaTc)))
                                           .Select(MapToDto).ToListAsync(cancellationToken);
            }

            return tree;
        }
    }
}