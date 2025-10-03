using CrazyRisk.Modelo;

public static class CreadorMapa
{
    public static Mapa CrearMapaMundial()
    {
        Mapa mapa = new Mapa();

        // 1. Crear todos los objetos Territorio
        //asia
        var japon = new Territorio("Japon", "asia");
        var coreaSur = new Territorio("Corea del Sur", "asia");
        var coreaNorte = new Territorio("Corea del Norte", "asia");
        var china = new Territorio("china", "asia");
        var mongolia = new Territorio("Mongolia", "asia");
        var india = new Territorio("India", "asia");
        var rusia = new Territorio("Rusia", "asia");
        var qatar = new Territorio("Qatar", "asia");
        var arabiaSaudita = new Territorio("Arabia Saudita", "asia");
        var iran = new Territorio("Iran", "asia");
        var turquia = new Territorio("Turquia", "asia");
        var singapur = new Territorio("Singapur", "asia");
        var indonesia = new Territorio("Indonesia", "asia");
        //europa
        var portugal = new Territorio("Portugal", "europa");
        var españa = new Territorio("España", "europa");
        var francia = new Territorio("Francia", "europa");
        var reinoUnido = new Territorio("Reino Unido", "europa");
        var alemania = new Territorio("Alemania", "europa");
        var italia = new Territorio("Italia", "europa");
        var polonia = new Territorio("Polonia", "europa");
        var croacia = new Territorio("Croacia", "europa");
        var paisesBajo = new Territorio("Paises bajo", "europa");
        var hungria = new Territorio("Hungría", "europa");
        //africa
        var egipto = new Territorio("Egipto", "africa");
        var sudafrica = new Territorio("Sudafrica", "africa");
        var nigeria = new Territorio("Nigeria", "africa");
        var kenia = new Territorio("Kenia", "africa");
        var congo = new Territorio("Congo", "africa");
        var marruecos = new Territorio("Marruecos", "africa");
        //America del Norte
        var eeuu = new Territorio("Estados Unidos", "America del Norte");
        var canada = new Territorio("Canada", "America del Norte");
        var mexico = new Territorio("Mexico", "America del Norte");
        var groenlandia = new Territorio("Groenlandia", "America del Norte");
        var costaRica = new Territorio("Costa Rica", "America del Norte");
        var panama = new Territorio("Panama", "America del Norte");
        //America del Sur
        var argentina = new Territorio("Argentina", "America del Sur");
        var brasil = new Territorio("Brasil", "America del Sur");
        var chile = new Territorio("Chile", "America del Sur");
        var uruguay = new Territorio("Uruguay", "America del Sur");
        //Oceania
        var australia = new Territorio("Australia", "Oceania");
        var nuevaZelanda = new Territorio("Nueva Zelanda", "Oceania");
        var papuaNuevaGuinea = new Territorio("papuaNuevaGuinea", "Oceania");

       // 2. Crear los 6 Continentes con su bonificación de refuerzos
        var asia = new Continente("Asia", 7);
        var europa = new Continente("Europa", 5);
        var africa = new Continente("Africa", 3);
        var america_norte = new Continente("América del Norte", 3);
        var america_sur = new Continente("América del Sur", 2);
        var oceania = new Continente("Oceanía", 2);


        // 3. Asignar cada Territorio a su Continente y al mapa general
        //asia
        asia.AddTerritorio(japon);
        mapa.AddTerritorio(japon);
        asia.AddTerritorio(coreaSur);
        mapa.AddTerritorio(coreaSur);
        asia.AddTerritorio(coreaNorte);
        mapa.AddTerritorio(coreaNorte);
        asia.AddTerritorio(china);
        mapa.AddTerritorio(china);
        asia.AddTerritorio(mongolia);
        mapa.AddTerritorio(mongolia);
        asia.AddTerritorio(india);
        mapa.AddTerritorio(india);
        asia.AddTerritorio(rusia);
        mapa.AddTerritorio(rusia);
        asia.AddTerritorio(qatar);
        mapa.AddTerritorio(qatar);
        asia.AddTerritorio(arabiaSaudita);
        mapa.AddTerritorio(arabiaSaudita);
        asia.AddTerritorio(iran);
        mapa.AddTerritorio(iran);
        asia.AddTerritorio(turquia);
        mapa.AddTerritorio(turquia);
        asia.AddTerritorio(singapur);
        mapa.AddTerritorio(singapur);
        asia.AddTerritorio(indonesia);
        mapa.AddTerritorio(indonesia);
        mapa.AddContinente(asia);

        //europa   
        europa.AddTerritorio(portugal);
        mapa.AddTerritorio(portugal);
        europa.AddTerritorio(españa);
        mapa.AddTerritorio(españa);
        europa.AddTerritorio(francia);
        mapa.AddTerritorio(francia);
        europa.AddTerritorio(reinoUnido);
        mapa.AddTerritorio(reinoUnido);
        europa.AddTerritorio(alemania);
        mapa.AddTerritorio(alemania);
        europa.AddTerritorio(italia);
        mapa.AddTerritorio(italia);
        europa.AddTerritorio(polonia);
        mapa.AddTerritorio(polonia);
        europa.AddTerritorio(croacia);
        mapa.AddTerritorio(croacia);
        europa.AddTerritorio(paisesBajo);
        mapa.AddTerritorio(paisesBajo);
        europa.AddTerritorio(hungria);
        mapa.AddTerritorio(hungria);
        mapa.AddContinente(europa);

        //africa
        africa.AddTerritorio(egipto);
        mapa.AddTerritorio(egipto);
        africa.AddTerritorio(sudafrica);
        mapa.AddTerritorio(sudafrica);
        africa.AddTerritorio(nigeria);
        mapa.AddTerritorio(nigeria);
        africa.AddTerritorio(kenia);
        mapa.AddTerritorio(kenia);
        africa.AddTerritorio(congo);
        mapa.AddTerritorio(congo);
        africa.AddTerritorio(marruecos);
        mapa.AddTerritorio(marruecos);
        mapa.AddContinente(africa);

        //America del Norte
        america_norte.AddTerritorio(eeuu);
        mapa.AddTerritorio(eeuu);
        america_norte.AddTerritorio(canada);
        mapa.AddTerritorio(canada);
        america_norte.AddTerritorio(mexico);
        mapa.AddTerritorio(mexico);
        america_norte.AddTerritorio(groenlandia);
        mapa.AddTerritorio(groenlandia);
        america_norte.AddTerritorio(costaRica);
        mapa.AddTerritorio(costaRica);
        america_norte.AddTerritorio(panama);
        mapa.AddTerritorio(panama);
        mapa.AddContinente(america_norte);

        //America del Sur
        america_sur.AddTerritorio(argentina);
        mapa.AddTerritorio(argentina);
        america_sur.AddTerritorio(brasil);
        mapa.AddTerritorio(brasil);
        america_sur.AddTerritorio(chile);
        mapa.AddTerritorio(chile);
        america_sur.AddTerritorio(uruguay);
        mapa.AddTerritorio(uruguay);
        mapa.AddContinente(america_sur);

        //Oceania
        oceania.AddTerritorio(australia);
        mapa.AddTerritorio(australia);
        oceania.AddTerritorio(nuevaZelanda);
        mapa.AddTerritorio(nuevaZelanda);
        oceania.AddTerritorio(papuaNuevaGuinea);
        mapa.AddTerritorio(papuaNuevaGuinea);
        mapa.AddContinente(oceania);

        // 4. Definir las conexiones entre territorios
        // ===============================================
        // América del Norte
        // ===============================================
        groenlandia.AgregarAdyacente(canada);
        groenlandia.AgregarAdyacente(eeuu);

        canada.AgregarAdyacente(groenlandia);
        canada.AgregarAdyacente(eeuu);
        canada.AgregarAdyacente(mexico);

        eeuu.AgregarAdyacente(groenlandia);
        eeuu.AgregarAdyacente(canada);
        eeuu.AgregarAdyacente(mexico);
        eeuu.AgregarAdyacente(costaRica);
        eeuu.AgregarAdyacente(panama);

        mexico.AgregarAdyacente(canada);
        mexico.AgregarAdyacente(eeuu);
        mexico.AgregarAdyacente(panama);
        mexico.AgregarAdyacente(francia);

        costaRica.AgregarAdyacente(eeuu);
        costaRica.AgregarAdyacente(panama);
        costaRica.AgregarAdyacente(brasil);

        panama.AgregarAdyacente(costaRica);
        panama.AgregarAdyacente(eeuu);
        panama.AgregarAdyacente(mexico);

        // ===============================================
        // América del Sur
        // ===============================================
        brasil.AgregarAdyacente(costaRica);
        brasil.AgregarAdyacente(panama);
        brasil.AgregarAdyacente(argentina);
        brasil.AgregarAdyacente(chile);

        argentina.AgregarAdyacente(brasil);
        argentina.AgregarAdyacente(chile);
        argentina.AgregarAdyacente(uruguay);

        chile.AgregarAdyacente(brasil);
        chile.AgregarAdyacente(argentina);
        chile.AgregarAdyacente(uruguay);

        uruguay.AgregarAdyacente(argentina);
        uruguay.AgregarAdyacente(chile);
        uruguay.AgregarAdyacente(marruecos);

        // ===============================================
        // Europa
        // ===============================================
        reinoUnido.AgregarAdyacente(portugal);
        reinoUnido.AgregarAdyacente(francia);

        portugal.AgregarAdyacente(reinoUnido);
        portugal.AgregarAdyacente(españa);
        portugal.AgregarAdyacente(paisesBajo);

        españa.AgregarAdyacente(portugal);
        españa.AgregarAdyacente(alemania);

        francia.AgregarAdyacente(mexico);
        francia.AgregarAdyacente(reinoUnido);
        francia.AgregarAdyacente(paisesBajo);
        francia.AgregarAdyacente(italia);

        paisesBajo.AgregarAdyacente(portugal);
        paisesBajo.AgregarAdyacente(francia);
        paisesBajo.AgregarAdyacente(croacia);
        paisesBajo.AgregarAdyacente(alemania);

        alemania.AgregarAdyacente(españa);
        alemania.AgregarAdyacente(paisesBajo);
        alemania.AgregarAdyacente(polonia);
        alemania.AgregarAdyacente(hungria);

        polonia.AgregarAdyacente(alemania);
        polonia.AgregarAdyacente(rusia);

        italia.AgregarAdyacente(francia);
        italia.AgregarAdyacente(croacia);
        italia.AgregarAdyacente(marruecos);
        italia.AgregarAdyacente(uruguay);

        croacia.AgregarAdyacente(paisesBajo);
        croacia.AgregarAdyacente(italia);
        croacia.AgregarAdyacente(hungria);

        hungria.AgregarAdyacente(croacia);
        hungria.AgregarAdyacente(alemania);

        // ===============================================
        // África
        // ===============================================
        marruecos.AgregarAdyacente(uruguay);
        marruecos.AgregarAdyacente(italia);
        marruecos.AgregarAdyacente(egipto);
        marruecos.AgregarAdyacente(nigeria);

        egipto.AgregarAdyacente(marruecos);
        egipto.AgregarAdyacente(nigeria);
        egipto.AgregarAdyacente(congo);

        nigeria.AgregarAdyacente(marruecos);
        nigeria.AgregarAdyacente(egipto);
        nigeria.AgregarAdyacente(congo);
        nigeria.AgregarAdyacente(kenia);
        nigeria.AgregarAdyacente(sudafrica);

        congo.AgregarAdyacente(egipto);
        congo.AgregarAdyacente(nigeria);
        congo.AgregarAdyacente(sudafrica);
        congo.AgregarAdyacente(rusia);

        kenia.AgregarAdyacente(nigeria);
        kenia.AgregarAdyacente(sudafrica);

        sudafrica.AgregarAdyacente(kenia);
        sudafrica.AgregarAdyacente(nigeria);
        sudafrica.AgregarAdyacente(congo);

        // ===============================================
        // Asia
        // ===============================================
        turquia.AgregarAdyacente(iran);
        turquia.AgregarAdyacente(rusia);

        iran.AgregarAdyacente(turquia);
        iran.AgregarAdyacente(arabiaSaudita);
        iran.AgregarAdyacente(mongolia);

        arabiaSaudita.AgregarAdyacente(iran);
        arabiaSaudita.AgregarAdyacente(qatar);
        arabiaSaudita.AgregarAdyacente(china);

        qatar.AgregarAdyacente(arabiaSaudita);
        qatar.AgregarAdyacente(india);

        rusia.AgregarAdyacente(congo);
        rusia.AgregarAdyacente(polonia);
        rusia.AgregarAdyacente(turquia);
        rusia.AgregarAdyacente(mongolia);
        rusia.AgregarAdyacente(coreaSur);

        mongolia.AgregarAdyacente(iran);
        mongolia.AgregarAdyacente(rusia);
        mongolia.AgregarAdyacente(china);
        mongolia.AgregarAdyacente(japon);

        china.AgregarAdyacente(arabiaSaudita);
        china.AgregarAdyacente(mongolia);
        china.AgregarAdyacente(india);
        china.AgregarAdyacente(indonesia);

        india.AgregarAdyacente(qatar);
        india.AgregarAdyacente(china);
        india.AgregarAdyacente(coreaNorte);
        india.AgregarAdyacente(indonesia);

        coreaNorte.AgregarAdyacente(india);
        coreaNorte.AgregarAdyacente(coreaSur);

        coreaSur.AgregarAdyacente(rusia);
        coreaSur.AgregarAdyacente(coreaNorte);
        coreaSur.AgregarAdyacente(japon);

        japon.AgregarAdyacente(mongolia);
        japon.AgregarAdyacente(coreaSur);
        japon.AgregarAdyacente(singapur);

        singapur.AgregarAdyacente(japon);
        singapur.AgregarAdyacente(china);
        singapur.AgregarAdyacente(indonesia);

        indonesia.AgregarAdyacente(singapur);
        indonesia.AgregarAdyacente(india);

        // ===============================================
        // Oceanía
        // ===============================================
        australia.AgregarAdyacente(rusia);
        australia.AgregarAdyacente(papuaNuevaGuinea);

        papuaNuevaGuinea.AgregarAdyacente(australia);
        papuaNuevaGuinea.AgregarAdyacente(nuevaZelanda);

        nuevaZelanda.AgregarAdyacente(papuaNuevaGuinea);


        

        return mapa;
    }
}