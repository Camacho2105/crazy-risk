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
        var papuaNuevaGuinea = new Territorio("Papua Nueva Guinea", "Oceania");

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
        //-- Conexiones en América del Norte --
        groenlandia.AgregarAdyacente(canada);
        groenlandia.AgregarAdyacente(reinoUnido); // Ruta marítima
        canada.AgregarAdyacente(eeuu);
        canada.AgregarAdyacente(groenlandia);
        eeuu.AgregarAdyacente(canada);
        eeuu.AgregarAdyacente(mexico);
        mexico.AgregarAdyacente(eeuu);
        mexico.AgregarAdyacente(costaRica);
        costaRica.AgregarAdyacente(mexico);
        costaRica.AgregarAdyacente(panama);
        panama.AgregarAdyacente(costaRica);
        panama.AgregarAdyacente(brasil); // Conexión a Sudamérica

        //-- Conexiones en América del Sur --
        brasil.AgregarAdyacente(panama);
        brasil.AgregarAdyacente(argentina);
        brasil.AgregarAdyacente(chile);
        brasil.AgregarAdyacente(uruguay);
        brasil.AgregarAdyacente(nigeria); // Ruta marítima a África
        argentina.AgregarAdyacente(brasil);
        argentina.AgregarAdyacente(chile);
        argentina.AgregarAdyacente(uruguay);
        chile.AgregarAdyacente(brasil);
        chile.AgregarAdyacente(argentina);
        uruguay.AgregarAdyacente(brasil);
        uruguay.AgregarAdyacente(argentina);

        //-- Conexiones en Europa --
        reinoUnido.AgregarAdyacente(groenlandia); // Ruta marítima
        reinoUnido.AgregarAdyacente(francia); // Ruta marítima
        reinoUnido.AgregarAdyacente(paisesBajo); // Ruta marítima
        portugal.AgregarAdyacente(españa);
        portugal.AgregarAdyacente(marruecos); // Ruta marítima
        españa.AgregarAdyacente(portugal);
        españa.AgregarAdyacente(francia);
        francia.AgregarAdyacente(españa);
        francia.AgregarAdyacente(reinoUnido);
        francia.AgregarAdyacente(alemania);
        francia.AgregarAdyacente(italia);
        paisesBajo.AgregarAdyacente(reinoUnido);
        paisesBajo.AgregarAdyacente(alemania);
        alemania.AgregarAdyacente(francia);
        alemania.AgregarAdyacente(paisesBajo);
        alemania.AgregarAdyacente(polonia);
        alemania.AgregarAdyacente(italia);
        italia.AgregarAdyacente(francia);
        italia.AgregarAdyacente(alemania);
        italia.AgregarAdyacente(croacia);
        polonia.AgregarAdyacente(alemania);
        polonia.AgregarAdyacente(rusia);
        polonia.AgregarAdyacente(hungria);
        croacia.AgregarAdyacente(italia);
        croacia.AgregarAdyacente(hungria);
        hungria.AgregarAdyacente(polonia);
        hungria.AgregarAdyacente(croacia);
        hungria.AgregarAdyacente(turquia);

        //-- Conexiones en África --
        marruecos.AgregarAdyacente(portugal);
        marruecos.AgregarAdyacente(egipto);
        egipto.AgregarAdyacente(marruecos);
        egipto.AgregarAdyacente(nigeria);
        egipto.AgregarAdyacente(kenia);
        egipto.AgregarAdyacente(arabiaSaudita); // Conexión a Asia
        nigeria.AgregarAdyacente(brasil);
        nigeria.AgregarAdyacente(egipto);
        nigeria.AgregarAdyacente(congo);
        congo.AgregarAdyacente(nigeria);
        congo.AgregarAdyacente(kenia);
        congo.AgregarAdyacente(sudafrica);
        kenia.AgregarAdyacente(egipto);
        kenia.AgregarAdyacente(congo);
        kenia.AgregarAdyacente(sudafrica);
        sudafrica.AgregarAdyacente(congo);
        sudafrica.AgregarAdyacente(kenia);
        sudafrica.AgregarAdyacente(australia); // Ruta marítima

        //-- Conexiones en Asia --
        turquia.AgregarAdyacente(hungria);
        turquia.AgregarAdyacente(rusia);
        turquia.AgregarAdyacente(iran);
        rusia.AgregarAdyacente(polonia);
        rusia.AgregarAdyacente(turquia);
        rusia.AgregarAdyacente(iran);
        rusia.AgregarAdyacente(mongolia);
        rusia.AgregarAdyacente(china);
        rusia.AgregarAdyacente(coreaNorte);
        arabiaSaudita.AgregarAdyacente(egipto);
        arabiaSaudita.AgregarAdyacente(qatar);
        arabiaSaudita.AgregarAdyacente(iran);
        qatar.AgregarAdyacente(arabiaSaudita);
        iran.AgregarAdyacente(turquia);
        iran.AgregarAdyacente(rusia);
        iran.AgregarAdyacente(arabiaSaudita);
        iran.AgregarAdyacente(india);
        india.AgregarAdyacente(iran);
        india.AgregarAdyacente(china);
        india.AgregarAdyacente(singapur);
        china.AgregarAdyacente(rusia);
        china.AgregarAdyacente(mongolia);
        china.AgregarAdyacente(coreaNorte);
        china.AgregarAdyacente(india);
        mongolia.AgregarAdyacente(rusia);
        mongolia.AgregarAdyacente(china);
        mongolia.AgregarAdyacente(japon); // Ruta marítima
        coreaNorte.AgregarAdyacente(rusia);
        coreaNorte.AgregarAdyacente(china);
        coreaNorte.AgregarAdyacente(coreaSur);
        coreaSur.AgregarAdyacente(coreaNorte);
        coreaSur.AgregarAdyacente(japon); // Ruta marítima
        japon.AgregarAdyacente(coreaSur);
        japon.AgregarAdyacente(mongolia);
        singapur.AgregarAdyacente(india);
        singapur.AgregarAdyacente(indonesia);
        indonesia.AgregarAdyacente(singapur);
        indonesia.AgregarAdyacente(australia);
        indonesia.AgregarAdyacente(papuaNuevaGuinea);

        //-- Conexiones en Oceanía --
        australia.AgregarAdyacente(sudafrica);
        australia.AgregarAdyacente(indonesia);
        australia.AgregarAdyacente(papuaNuevaGuinea);
        australia.AgregarAdyacente(nuevaZelanda); // Ruta marítima
        papuaNuevaGuinea.AgregarAdyacente(indonesia);
        papuaNuevaGuinea.AgregarAdyacente(australia);
        nuevaZelanda.AgregarAdyacente(australia);

        // No olvides agregar todos los continentes al mapa al final
        mapa.AddContinente(asia);
        mapa.AddContinente(europa);
        mapa.AddContinente(africa);
        mapa.AddContinente(america_norte);
        mapa.AddContinente(america_sur);
        mapa.AddContinente(oceania);
        
        

        return mapa;
    }
}