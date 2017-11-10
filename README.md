Rik.StatusPage
==============

Üldise otstarbega `HttpModule`, mis hõlbustab AspNet rakendusele RIK-i staatuselehe lisamist.


Kasutamine
----------

* Lisada veebirakenduse projektile `Rik.StatusPage` paki sõltuvus.

* Vastavalt IIS-i versioonile seadistada `Web.config` failis `Rik.StatusPage.StatusModule` laiendus.

  ```xml
  <configuration>
    <system.web>
      <httpModules>
        <add name="RikStatusPageModule" type="Rik.StatusPage.StatusModule, Rik.StatusPage" />
      </httpModules>
    </system.web>
  </configuration>

  <configuration>
    <system.webServer>
      <modules>
        <add name="RikStatusPageModule" type="Rik.StatusPage.StatusModule, Rik.StatusPage" />
      </modules>
    </system.webServer>
  </configuration>
  ```

* Täiendada `Global.asax.cs` või mujal asuvat globaalset `HttpApplication` objekti laiendavat klassi järgmiselt:

    * Mässida `Application_Start` loogika `Rik.StatusPage.StatusModule.CaptureApplicationStartErrors` meetodisse:

      ```csharp
      protected void Application_Start()
      {
          StatusModule.CaptureApplicationStartErrors(this, () =>
          {
              // For example:

              // Register IoC container

              AreaRegistration.RegisterAllAreas();
              RouteConfig.RegisterRoutes(RouteTable.Routes);

              // Rest of application initialization logic.
          });
      }
      ```

    * Lisada tingimus `Application_EndRequest` meetodisse:

      ```csharp
      protected void Application_EndRequest()
      {
          if (StatusModule.IsStatusPage(Context) || StatusModule.IsApplicationStartFailure(this))
              return;

          container.GetInstance<ISessionHandler>().CloseSession();
      }
      ```

Peale kirjeldatud muudatuste tegemist hakkab rakendus oma seisukorra kohta pakkuma infot asukohas `/status.xml`.


Välised sõltuvused
------------------

Rakenduse väliste sõltuvuste oleku kontrollimiseks on võimalik valida pakutavate *provider*-ite hulgast või
vajadusel defineerida enda omasid koodis. Antud paketi poolt pakutavate *provider*-ite loetelu on kirjeldatud allpool.

Välise sõltuvuse kirjeldamiseks tuleb rakenduse `Web.config` failis kirjeldada `rik.statuspage`
konfiguratsioonisektsioon, sarnaselt alljärgnevale näitele:

```xml
<configuration>
  <configSections>
    <section name="rik.statuspage" type="Rik.StatusPage.Configuration.StatusPageConfigurationSection, Rik.StatusPage" />
  </configSections>

  <appSettings>
    <add key="Turvaserver.Url" value="http://turvaserv.er/">
  </appSettings>

  <rik.statuspage>
    <application name="ApplicationName" />
    <statusProviders>
      <statusProvider name="Andmebaas" type="MsSqlDatabase" connectionString="Data Source=..." />
      <statusProvider name="Turvaserver" type="WebService" url="${Turvaserver.Url}" />
    </statusProviders>
  </rik.statuspage>
</configuration>
```

Nagu näidiselt näha, saab soovi korral *statusProvider* parameetri väärtusena viidata `appSettings` sektsioonis kirjeldatud
väärtustele nime `key` atribuudi alusel, kasutades `${...}` konstruktsiooni.


Väliste sõltuvuste *provider*-id
--------------------------------

### FileStorage

Kasutatakse failikataloogi juurdepääsu kontrollimiseks:

```xml
<statusProvider name="somefile" type="FileStorage" storagePath="C:\Temp" requireRead="true" requireWrite="false" />
```

* `storagePath` (kohustuslik parameeter) - failikataloogi asukoht
* `requireRead` (vaikimisi *false*) - nõuab lugemisõigust
* `requireWrite` (vaikimisi *false*) - nõuab kirjutamisõigust

### MsSqlDatabase

Kasutatakse *Microsoft Sql Server*-i juurdepääsu kontrollimiseks:

```xml
<statusProvider name="some important database" type="MsSqlDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### OracleDatabase

Kasutatakse *Oracle* andmebaasi juurdepääsu kontrollimiseks:

```xml
<statusProvider name="some important database" type="OracleDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### PostgreDatabase

Kasutatakse *PostgreSQL* andmebaasi juurdepääsu kontrollimiseks:

```xml
<statusProvider name="some important database" type="PostgreDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### WebService

Kasutatakse veebiteenuse juurdepääsu kontrollimiseks (HTTP GET):

```xml
<statusProvider name="some important database" type="PostgreDatabase" url="..." />
```

* `url` (kohustuslik parameeter) - veebiteenuse aadress

### XRoadProducer

Kasutatakse X-tee andmekogu juurdepääsu kontrollimiseks (getState metateenus):

```xml
<statusProvider name="producer name" type="XRoadProducer" protocol="3.1" securityServer="..." producerName="..." consumer="..." userId="..." />
```

* `protocol` (kohustuslik parameeter) - kasutatav X-tee sõnumivahetuse protokoll (`"2.0"`, `"3.0"`, `"3.1"`, `"4.0"`)
* `securityServer` (kohustuslik parameeter) - kasutatava turvaserveri url
* `producerName` (kohustuslik parameeter) - andmekogu nimi
* `consumer` (kohustuslik parameeter) - klientrakenduse andmekogu nimi
* `userId` (kohustuslik parameeter) - veebiteenuse kasutaja kood (kasutatakse X-tee päises)

### Custom

Kasutatakse rakenduse poolt defineeritud *provider*-i kirjeldamiseks:

```xml
<statusProvider name="producer name" type="Custom" class="My.Web.App.RandomStatusProvider, My.Web.App" />
```

* `class` (kohustuslik parameeter) - rakenduses kirjeldatud andmetüüp, mis realiseerib soovitud kontrolli.

### SinuEndaKlots

Kui sulle tundub, et oled leiutanud väärt *provider*-i, millest võiks ka teistele kasu tõusta, siis jaga oma
loomingut *Pull Request*-iga.
