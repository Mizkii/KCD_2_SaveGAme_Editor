// =====================================================================
// KINGDOM COME: DELIVERANCE II - SAVE GAME EDITOR
// GitHub Pages Interactive Script & Bilingual Engine
// =====================================================================

const TRANSLATIONS = {
    en: {
        nav_features: "Features",
        nav_gallery: "Screenshots",
        nav_specs: "Architecture",
        nav_faq: "FAQ",
        nav_github: "View on GitHub",
        nav_download: "Download v2.5.0",
        
        hero_tag: "⚔️ NEXT-GEN SAVE EDITOR FOR KCD2",
        hero_title: "Kingdom Come: Deliverance II<br>SaveGame Editor",
        hero_subtitle: "The ultimate native C++20 core & .NET 10 WPF editor for Kingdom Come: Deliverance II. Full support for Steam & Xbox Game Pass saves, 5,604 item database, triple outfits, companion management, and quest fate controls.",
        hero_btn_release: "⬇️ Download Latest Release",
        hero_btn_source: "⭐ Star on GitHub",
        
        badge_platform: "Steam & Xbox Game Pass",
        badge_version: "v2.5.0-PRO",
        badge_build: "C++20 & .NET 10",
        badge_items: "5,604 Items Codex",
        
        section_features_tag: "Comprehensive Capabilities",
        section_features_title: "Crafted for True Medieval Immersion",
        section_features_desc: "Every parameter reverse-engineered from native game binaries with mathematical precision, automatic CRC/MD5 verification, and fail-safe backup guarantees.",
        
        feat_1_title: "Paperdoll & Triple Outfits",
        feat_1_desc: "16-slot layered armor hierarchy (Gorget, Coif, Bascinet, Chausses, Cuirass, Signet Ring). Instant 1-click swapping between Combat, Town, and Hunting loadouts with dynamic slash/pierce/blunt defense ratings.",
        
        feat_2_title: "Horse Tack & Dog Companion",
        feat_2_desc: "Manage Pebbles or any acquired steed with 6 modular tack slots (Saddle, Caparison, Horseshoes, Chanfron, Saddlebags) and 3 Tack Presets. Full Mutt houndmaster perks, loyalty, obedience, and nourishment sliders.",
        
        feat_3_title: "Multi-Storage & 5,604 Item Codex",
        feat_3_desc: "Switch between Henry, Stash, and Horse saddlebags with 14 category filter pills. Spawn any official item with bilingual names, adjust condition, blood/dirt, duplicate, and legalize stolen items with zero trace.",
        
        feat_4_title: "Quest Log & NPC Fate Resurrector",
        feat_4_desc: "Unlock, reset, or advance all Main Quests, Side Quests, and Activities. Dedicated 'Point of No Return' detection and Key NPC tracker (Hans Capon, Radzig, Theresa) with 1-click dead NPC resurrection.",
        
        feat_5_title: "Herbalism & Alchemy Autobrew",
        feat_5_desc: "Master Blacksmithing with 100% blade sharpening and tempering. Complete herbalism pouch editor (+50 or Max 999 for all herbs) and instant unlock of 92 crafting recipes and auto-brew perks.",
        
        feat_6_title: "Fail-Safe Save Integrity",
        feat_6_desc: "High-performance C++20 core (KCD2SaveCore.dll) with miniz Deflate compression, header parsing, auto-backup creation (.bak), and native WGS / Steam path auto-detection.",
        
        section_gallery_tag: "Visual Walkthrough",
        section_gallery_title: "Editor Feature Screenshots",
        section_gallery_desc: "Explore high-resolution captures of every screen and dialog in the editor.",
        
        filter_all: "All Screens",
        filter_player: "Player & Outfits",
        filter_horse: "Horse & Companions",
        filter_inventory: "Inventory & Codex",
        filter_quests: "Quests & Fates",
        filter_crafting: "Crafting & Alchemy",
        
        gallery_01_title: "Henry Paperdoll & Outfits",
        gallery_01_desc: "16-slot equipment inspector, Outfit I/II/III preset buttons, defense matrix and RPG attributes.",
        
        gallery_02_title: "Slot Equipment Selector Modal",
        gallery_02_desc: "Gothic modal showing 435 matching helmets from codex and inventory with live search and stats.",
        
        gallery_03_title: "Horse Tack Equipment & Outfits",
        gallery_03_desc: "6 interactive tack slots, 3 Tack Presets (Streitross, Reise, Prunk), dynamic speed and capacity.",
        
        gallery_04_title: "Horse Tack Selector Modal",
        gallery_04_desc: "153 saddles and tack gear from official game database with price, tier, and weight preview.",
        
        gallery_05_title: "Character Stats & Reputation",
        gallery_05_desc: "Factions reputation sliders (Kuttenberg, Trosky, Suchdol) and RPG perk progression tree.",
        
        gallery_06_title: "Mutt Dog Companion Manager",
        gallery_06_desc: "Mutt's loyalty, obedience, aggression sliders, hunting/watchdog perks, and Maximize Mutt button.",
        
        gallery_07_title: "Mount Stats & Horsemanship",
        gallery_07_desc: "Direct speed, capacity, courage, and stamina fine-tuning with Max Stats preset.",
        
        gallery_08_title: "Codex Item Spawner Dialog",
        gallery_08_desc: "Browse 5,604 official items by category, price, destination container, quantity, and stolen state.",
        
        gallery_09_title: "Quest Log & Stage Objectives",
        gallery_09_desc: "Full quest progression tree with 'Point of No Return' badges and active stage checklists.",
        
        gallery_10_title: "NPC Fates & Resurrector",
        gallery_10_desc: "Track living, dead, or missing story characters and instantly revive fallen allies.",
        
        gallery_11_title: "Crafting, Alchemy & Herbs Pouch",
        gallery_11_desc: "92 alchemy and smithing recipes, instant auto-brew, and 10-slot herbalism pouch management.",
        
        gallery_12_title: "German Localization UI",
        gallery_12_desc: "100% native German translation for all 5,604 items, quest names, perks, and interface labels.",
        
        gallery_13_title: "English Localization UI",
        gallery_13_desc: "100% native English translation with automated save synchronization and MD5 verification.",
        
        section_tech_tag: "Native Engineering",
        section_tech_title: "Modern Dual-Tier Architecture",
        section_tech_desc: "Engineered from the ground up for zero crashes, lightning-fast parsing, and clean modular code.",
        
        tech_core_title: "C++20 Native Engine (KCD2SaveCore.dll)",
        tech_core_1_k: "Standard", tech_core_1_v: "ISO C++20 Standard",
        tech_core_2_k: "Compression", tech_core_2_v: "Optimized miniz Deflate",
        tech_core_3_k: "Hashing", tech_core_3_v: "MD5 Verification & Checksum Patching",
        tech_core_4_k: "Containers", tech_core_4_v: "WHS / Zip 100% Binary Safe",
        tech_core_5_k: "Target", tech_core_5_v: "Native Win-x64 ABI Export",
        
        tech_ui_title: "WPF Frontend (.NET 10.0)",
        tech_ui_1_k: "Framework", tech_ui_1_v: ".NET 10.0-windows (Modern WPF)",
        tech_ui_2_k: "Pattern", tech_ui_2_v: "MVVM with WeakReference Messaging",
        tech_ui_3_k: "Database", tech_ui_3_v: "5,604 Items Official Game Codex",
        tech_ui_4_k: "P/Invoke", tech_ui_4_v: "Direct Fast Native C-Bindings",
        tech_ui_5_k: "CI/CD", tech_ui_5_v: "Automated GitHub Actions Build",
        
        section_faq_tag: "Frequently Asked Questions",
        section_faq_title: "Everything You Need to Know",
        section_faq_desc: "Common questions about save compatibility, safety guarantees, and game mechanics.",
        
        faq_1_q: "Does this Save Editor support both Steam and Xbox Game Pass PC?",
        faq_1_a: "Yes! The editor features automatic platform detection. It locates standard Steam .whs files in <code>%USERPROFILE%\\Saved Games\\kingdomcome2\\saves\\</code> as well as Microsoft Store / Xbox Game Pass WGS (Windows Gaming Storage) encrypted container directories with zero manual configuration.",
        
        faq_2_q: "Can I resurrect dead story characters or fix broken quests?",
        faq_2_a: "Absolutely. The integrated Quest Log & Character Fates module allows you to track key NPCs (Sir Radzig, Sir Hans Capon, Father Bohuta, Theresa, etc.) and restore them to 'Alive' status in 1 click. You can also advance stalled objectives or reset quest states.",
        
        faq_3_q: "How does the 5,604-item Codex Spawner work?",
        faq_3_a: "The editor bundles the complete Kingdom Come: Deliverance II official item database. You can search by English or German names, filter by category (Weapons, Armor, Food, Potions, Horse Tack, Books), set condition to 100%, and choose whether to deposit into Henry's inventory, personal chest (stash), or horse saddlebags.",
        
        faq_4_q: "Will editing my save corrupt my files or get me banned?",
        faq_4_a: "No. The native C++20 engine strictly recalculates CryEngine's internal 64-byte XMLCPB trailer and RFC 1321 MD5 integrity checksums. Furthermore, every single save action automatically creates a timestamped safety backup (.bak), ensuring your original files are 100% protected.",
        
        faq_5_q: "Is this save editor free and open-source?",
        faq_5_a: "Yes, 100% free and open-source under the permissive MIT License. There are no ads, paywalls, or premium tiers. The source code and automated build workflows are publicly available on GitHub.",
        
        footer_disclaimer: "Kingdom Come: Deliverance II is a registered trademark of Warhorse Studios and PLAION. This open-source save game editor is an independent community project and is not affiliated with or endorsed by Warhorse Studios."
    },
    de: {
        nav_features: "Funktionen",
        nav_gallery: "Screenshots",
        nav_specs: "Architektur",
        nav_faq: "FAQ",
        nav_github: "GitHub Repository",
        nav_download: "v2.5.0 Herunterladen",
        
        hero_tag: "⚔️ DER ULTIMATIVE KCD2 SAVE EDITOR",
        hero_title: "Kingdom Come: Deliverance II<br>SaveGame Editor",
        hero_subtitle: "Der hochentwickelte native C++20 Core & .NET 10 WPF Spielstand-Editor für Kingdom Come: Deliverance II. Volle Unterstützung für Steam & Xbox Game Pass Spielstände, 5.604 Gegenstände Codex, 3 Outfit-Sets, Pferd & Hund Begleiter und Questlog-Schicksale.",
        hero_btn_release: "⬇️ Neueste Version herunterladen",
        hero_btn_source: "⭐ Auf GitHub ansehen",
        
        badge_platform: "Steam & Xbox Game Pass",
        badge_version: "v2.5.0-PRO",
        badge_build: "C++20 & .NET 10",
        badge_items: "5.604 Gegenstände Codex",
        
        section_features_tag: "Vollständige Funktionsübersicht",
        section_features_title: "Entwickelt für maximale Immersion",
        section_features_desc: "Jeder Parameter wurde mit nativer C++ Präzision aus den Spieldateien rekonstruiert, inklusive automatischer Prüfsummen-Korrektur und Ausfallsicherheits-Backups (.bak).",
        
        feat_1_title: "Paperdoll & 3 Outfit-Garnituren",
        feat_1_desc: "16 hierarchische Rüstungsslots (Halsberge, Beckenhaube, Beinlinge, Kürass, Siegelring). Blitzschneller 1-Klick Wechsel zwischen Kampf-, Stadt- und Jagd-Garnitur mit dynamischer Berechnung von Stich-, Hieb- und Schlag-Schutz.",
        
        feat_2_title: "Pferde-Ausstattung & Köter",
        feat_2_desc: "Verwalte Schimmel (Pebbles) oder jedes andere Reittier mit 6 interaktiven Slots (Sattel, Schabracke, Hufeisen, Rossharnisch, Packtaschen) und 3 Pferde-Garnituren. Volle Kontrolle über Köters Treue, Gehorsam und Jagd-Perks.",
        
        feat_3_title: "Lagerort-Wechsler & 5.604 Codex",
        feat_3_desc: "Wechsle nahtlos zwischen Heinrich, Truhe und Pferd mit 14 Schnellfiltern. Spawne jeden offiziellen Gegenstand mit deutschen Namen, passe Zustand und Blut/Dreck an und legalisiere Diebesgut rückstandslos.",
        
        feat_4_title: "Questlog & NPC Wiederbelebung",
        feat_4_desc: "Schalte Hauptquests, Nebenquests und Aktivitäten frei. Erkennung von Entscheidungen 'Ohne Wiederkehr' und NPC-Statusanzeige (Hans Capon, Herr Radzig, Theresa) mit 1-Klick-Wiederbelebung gefallener Charaktere.",
        
        feat_5_title: "Kräuterkunde & Meister-Alchemie",
        feat_5_desc: "Schmiedekunst mit 100% Klingen-Schärfung. Vollständiger Kräuterbeutel (+50 oder Max 999 für alle Kräuter) sowie sofortige Freischaltung von 92 Alchemie- und Handwerksrezepten mit Routine-Autobrew.",
        
        feat_6_title: "Ausfallsichere Spielstand-Integrität",
        feat_6_desc: "Hochperformanter C++20 Engine Core (KCD2SaveCore.dll) mit miniz Deflate-Kompression, Header-Analyse, automatischer .bak-Sicherung und automatischer Erkennung von Xbox WGS und Steam Verzeichnissen.",
        
        section_gallery_tag: "Visuelle Tour",
        section_gallery_title: "Screenshots aller Funktionen",
        section_gallery_desc: "Erkunde alle Ansichten, Dialoge und Werkzeuge des Editors in voller Auflösung.",
        
        filter_all: "Alle Ansichten",
        filter_player: "Spieler & Outfits",
        filter_horse: "Pferd & Begleiter",
        filter_inventory: "Inventar & Codex",
        filter_quests: "Quests & Schicksale",
        filter_crafting: "Handwerk & Alchemie",
        
        gallery_01_title: "Heinrich Paperdoll & Outfits",
        gallery_01_desc: "16 Rüstungsslots, Outfit I/II/III Buttons, Schutzmatrix und RPG-Attribute.",
        
        gallery_02_title: "Ausrüstungs-Auswahl Modal",
        gallery_02_desc: "Gothisches Auswahlfenster mit 435 Helmen aus Codex und Inventar samt Schnellsuche.",
        
        gallery_03_title: "Pferde-Ausstattung & Garnituren",
        gallery_03_desc: "6 interaktive Reittierslots, 3 Garnituren (Streitross, Reise, Prunk) und dynamische Werte.",
        
        gallery_04_title: "Pferde-Sattel Auswahl Modal",
        gallery_04_desc: "153 Sättel und Riemenzeug aus der Spieledatenbank mit Preis, Rang und Gewicht.",
        
        gallery_05_title: "Charakterwerte & Ruf",
        gallery_05_desc: "Fraktionsruf (Kuttenberg, Trosky, Suchdol) und RPG Perk-Baum.",
        
        gallery_06_title: "Köter Begleiter-Verwaltung",
        gallery_06_desc: "Köters Treue, Gehorsam, Aggression, Hundemeister-Perks und 'Köter maximieren' Button.",
        
        gallery_07_title: "Reitkunst & Pferdestats",
        gallery_07_desc: "Direkte Feinabstimmung von Geschwindigkeit, Tragkraft, Mut und Ausdauer.",
        
        gallery_08_title: "Codex Gegenstands-Spawner",
        gallery_08_desc: "5.604 offizielle Gegenstände nach Kategorie, Preis, Zielcontainer, Menge und Diebesgut filtern.",
        
        gallery_09_title: "Questlog & Missionsstufen",
        gallery_09_desc: "Missionsbaum mit 'Ohne Wiederkehr'-Warnung und Fortschritts-Checkliste.",
        
        gallery_10_title: "NPC Schicksale & Wiederbelebung",
        gallery_10_desc: "Wichtige Story-Charaktere überwachen und gefallene Verbündete wiederbeleben.",
        
        gallery_11_title: "Handwerk, Alchemie & Kräuterbeutel",
        gallery_11_desc: "92 Rezepte, Sofort-Autobrew und 10-Slot Kräuterbeutel-Verwaltung.",
        
        gallery_12_title: "Deutsche Benutzeroberfläche",
        gallery_12_desc: "Vollständige deutsche Lokalisierung für alle 5.604 Gegenstände, Quests und Werte.",
        
        gallery_13_title: "Englische Benutzeroberfläche",
        gallery_13_desc: "Vollständige englische Lokalisierung mit automatischer Synchronisation und MD5-Prüfung.",
        
        section_tech_tag: "Native Software-Architektur",
        section_tech_title: "Modernes Zwei-Schichten-Design",
        section_tech_desc: "Konzipiert für maximale Stabilität, blitzschnelles Parsen und saubere Wartbarkeit.",
        
        tech_core_title: "C++20 Native Engine (KCD2SaveCore.dll)",
        tech_core_1_k: "Standard", tech_core_1_v: "ISO C++20 Standard",
        tech_core_2_k: "Kompression", tech_core_2_v: "Optimiertes miniz Deflate",
        tech_core_3_k: "Hashing", tech_core_3_v: "MD5 Integrität & Checksummen-Patching",
        tech_core_4_k: "Container", tech_core_4_v: "WHS / Zip 100% binärsicher",
        tech_core_5_k: "Zielsystem", tech_core_5_v: "Natives Win-x64 ABI",
        
        tech_ui_title: "WPF Frontend (.NET 10.0)",
        tech_ui_1_k: "Framework", tech_ui_1_v: ".NET 10.0-windows (Modernes WPF)",
        tech_ui_2_k: "Pattern", tech_ui_2_v: "MVVM mit WeakReference Messaging",
        tech_ui_3_k: "Datenbank", tech_ui_3_v: "5.604 Gegenstände Codex",
        tech_ui_4_k: "P/Invoke", tech_ui_4_v: "Direkte Native C-Bindings",
        tech_ui_5_k: "CI/CD", tech_ui_5_v: "Automatisierter GitHub Actions Build",
        
        section_faq_tag: "Häufig gestellte Fragen",
        section_faq_title: "Alles was du wissen musst",
        section_faq_desc: "Wichtige Antworten zu Kompatibilität, Ausfallsicherheit und Funktionen.",
        
        faq_1_q: "Unterstützt dieser Save Editor sowohl Steam als auch Xbox Game Pass PC?",
        faq_1_a: "Ja! Der Editor verfügt über eine automatische Plattform-Erkennung. Er findet Standard Steam .whs Dateien in <code>%USERPROFILE%\\Saved Games\\kingdomcome2\\saves\\</code> sowie Microsoft Store / Xbox Game Pass WGS Container-Ordner vollautomatisch.",
        
        faq_2_q: "Kann ich tote NPCs wiederbeleben oder fehlerhafte Quests reparieren?",
        faq_2_a: "Absolut. Das integrierte Questlog- und Schicksale-Modul ermöglicht es, wichtige Story-Charaktere (Herr Radzig, Hans Capon, Pater Bohuta, Theresa etc.) mit einem Klick auf 'Lebendig' zurückzusetzen und blockierte Missionsziele fortzusetzen.",
        
        faq_3_q: "Wie funktioniert der 5.604-Items Codex Spawner?",
        faq_3_a: "Der Editor enthält die vollständige offizielle Spieledatenbank. Du kannst nach deutschen oder englischen Begriffen suchen, nach Kategorien filtern, 100% Haltbarkeit einstellen und Gegenstände direkt in Heinrichs Inventar, die Truhe oder Satteltaschen einfügen.",
        
        faq_4_q: "Können Spielstände beschädigt werden oder droht ein Bann?",
        faq_4_a: "Nein. Die native C++20 Engine berechnet die internen CryEngine 64-Byte XMLCPB Trailer und MD5 Prüfsummen exakt neu. Zudem erstellt jeder Speichervorgang automatisch ein zeitgestempeltes Backup (.bak).",
        
        faq_5_q: "Ist der KCD2 Save Editor wirklich kostenlos und quelloffen?",
        faq_5_a: "Ja, zu 100% kostenlos und Open-Source unter der MIT-Lizenz. Es gibt keinerlei Werbung, Paywalls oder Abos. Der Quellcode ist vollständig auf GitHub einsehbar.",
        
        footer_disclaimer: "Kingdom Come: Deliverance II ist ein eingetragenes Markenzeichen von Warhorse Studios und PLAION. Dieser Open-Source Savegame-Editor ist ein unabhängiges Community-Projekt und steht in keiner offiziellen Verbindung zu Warhorse Studios."
    }
};

let currentLang = 'en';

function setLanguage(lang) {
    currentLang = lang;
    const t = TRANSLATIONS[lang] || TRANSLATIONS.en;
    
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (t[key]) {
            el.innerHTML = t[key];
        }
    });

    const btn = document.getElementById('lang-switch-btn');
    if (btn) {
        btn.textContent = lang === 'en' ? '🌐 Deutsch' : '🌐 English';
    }
    
    document.documentElement.lang = lang;
}

function toggleLanguage() {
    setLanguage(currentLang === 'en' ? 'de' : 'en');
}

// Screenshot Filter Logic
function initGalleryFilters() {
    const buttons = document.querySelectorAll('.filter-btn');
    const items = document.querySelectorAll('.gallery-item');
    
    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            const filter = btn.getAttribute('data-filter');
            items.forEach(item => {
                if (filter === 'all' || item.getAttribute('data-category') === filter) {
                    item.style.display = 'flex';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    });
}

// Lightbox Logic
function initLightbox() {
    const modal = document.getElementById('lightbox-modal');
    const img = document.getElementById('lightbox-img');
    const title = document.getElementById('lightbox-title');
    const desc = document.getElementById('lightbox-desc');
    const closeBtn = document.getElementById('lightbox-close');
    
    document.querySelectorAll('.gallery-item').forEach(item => {
        item.addEventListener('click', () => {
            const thumb = item.querySelector('.gallery-thumb');
            const caption = item.querySelector('.gallery-caption');
            const subtext = item.querySelector('.gallery-subtext');
            
            img.src = thumb.src;
            title.textContent = caption.textContent;
            desc.textContent = subtext.textContent;
            modal.style.display = 'flex';
        });
    });

    const closeModal = () => {
        modal.style.display = 'none';
    };

    closeBtn.addEventListener('click', closeModal);
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'flex') {
            closeModal();
        }
    });
}

// FAQ Accordion Logic
function initFaqAccordion() {
    document.querySelectorAll('.faq-question').forEach(q => {
        q.addEventListener('click', () => {
            const item = q.parentElement;
            const wasActive = item.classList.contains('active');
            
            // Close others
            document.querySelectorAll('.faq-item').forEach(i => i.classList.remove('active'));
            
            if (!wasActive) {
                item.classList.add('active');
            }
        });
    });
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
    initGalleryFilters();
    initLightbox();
    initFaqAccordion();
    setLanguage('en');
    
    const langBtn = document.getElementById('lang-switch-btn');
    if (langBtn) {
        langBtn.addEventListener('click', toggleLanguage);
    }
});
