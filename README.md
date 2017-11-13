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
    <app name="ApplicationName" />
    <externalDependencies>
      <unit name="Andmebaas" provider="MsSqlDatabase" connectionString="Data Source=..." />
      <unit name="Turvaserver" provider="WebService" url="${Turvaserver.Url}" />
    </externalDependencies>
  </rik.statuspage>
</configuration>
```

Nagu näidiselt näha, saab soovi korral *statusProvider* parameetri väärtusena viidata `appSettings` sektsioonis kirjeldatud
väärtustele nime `key` atribuudi alusel, kasutades `${...}` konstruktsiooni.

### rik.statuspage sektsioon

Võimaldab soovi korral kasutada kahte alamelementi `app` ja `externalDependencies` staatuslehe seadistamiseks.

#### app

```xml
<rik.statuspage>
  <app name="Minu rakendus" version="3.0.1-beta001">
    <oluline-info>Oluline info staatuslehele</oluline-info>
  </app>
</rik.statuspage>
```

* `name` (atribuut) - Rakenduse nime seadistamiseks staatuselehe (vaikimisi rakenduse *assembly* nimi).
* `version` (atribuut) - Rakenduse versioon (vaikimisi rakenduse *assembly* versiooni number).
* Alamelemendid lisatakse muutmata kujul automaatselt staatuslehe `app` elemendile.

#### externalDependencies

Alamelementide loend kirjeldab ära väliste sõltuvuste üksused, mida saab seadistada järgmises lõigus kirjeldatud
*provider*-ite alusel.

```xml
<rik.statuspage>
  <externalDependencies>
    <unit name="Suvaline" provider="PostgreDatabase" uri="psql://host/db" connectionString=".." />
  </externalDependencies>
</rik.statuspage>
```

Üldised atribuudid:

* `name` (kohustuslik) - välise üksuse nimi staatuslehel.
* `provider` (kohustuslik) - *provider*-i nimi, mis vahendab üksuse staatuse kontrollimist.
* `uri` - võimaldab soovi korral üksuse `uri` elemendi väärtuse määramist, kui automaatne väärtus ei kata rakenduse vajadusi.

Väliste sõltuvuste *provider*-id
--------------------------------

### FileStorage

Kasutatakse failikataloogi juurdepääsu kontrollimiseks:

```xml
<unit name="somefile" provider="FileStorage" storagePath="C:\Temp" requireRead="true" requireWrite="false" />
```

* `storagePath` (kohustuslik parameeter) - failikataloogi asukoht
* `requireRead` (vaikimisi *false*) - nõuab lugemisõigust
* `requireWrite` (vaikimisi *false*) - nõuab kirjutamisõigust

### MsSqlDatabase

Kasutatakse *Microsoft Sql Server*-i juurdepääsu kontrollimiseks:

```xml
<unit name="some important database" provider="MsSqlDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### OracleDatabase

Kasutatakse *Oracle* andmebaasi juurdepääsu kontrollimiseks:

```xml
<unit name="some important database" provider="OracleDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### PostgreDatabase

Kasutatakse *PostgreSQL* andmebaasi juurdepääsu kontrollimiseks:

```xml
<unit name="some important database" provider="PostgreDatabase" connectionString="..." />
```

* `connectionString` (kohustuslik parameeter) - andmebaasi juurdepääsukirjeldus

### RabbitMq

Kasutatakse *Rabbit MQ* juurdepääsu ja staatuse kontrollimiseks. Eeldab, et rakendusele on lisatud viide
`EasyNetQ.Management.Client` paketile.

```xml
<unit name="Minu jänesed" provider="RabbitMq" connectionString="amqp://user:pass@server" />
```

* `connectionString` (kohustuslik parameeter) - *Rabbit MQ* juurdepääsukirjeldus

### WebService

Kasutatakse veebiteenuse juurdepääsu kontrollimiseks (HTTP GET):

```xml
<unit name="some important database" provider="WebService" url="..." />
```

* `url` (kohustuslik parameeter) - veebiteenuse aadress

### XRoadProducer

Kasutatakse X-tee andmekogu juurdepääsu kontrollimiseks (getState metateenus):

```xml
<unit name="producer name" provider="XRoadProducer" protocol="3.1" securityServer="..." producerName="..." consumer="..." userId="..." />
```

* `protocol` (kohustuslik parameeter) - kasutatav X-tee sõnumivahetuse protokoll (`"2.0"`, `"3.0"`, `"3.1"`, `"4.0"`)
* `securityServer` (kohustuslik parameeter) - kasutatava turvaserveri url
* `producerName` (kohustuslik parameeter) - andmekogu nimi
* `consumer` (kohustuslik parameeter) - klientrakenduse andmekogu nimi
* `userId` (kohustuslik parameeter) - veebiteenuse kasutaja kood (kasutatakse X-tee päises)

### Custom

Kasutatakse rakenduse poolt defineeritud *provider*-i kirjeldamiseks:

```xml
<unit name="Elastic" provider="Custom" class="My.Web.ElasticStatusProvider, My.Web" connectionString="http://elasticserver/" index="indexName" />
```

* `class` (kohustuslik parameeter) - rakenduses kirjeldatud andmetüüp, mis realiseerib soovitud kontrolli.

Rakenduse poolt defineeritud *provider* peaks laiendama `Rik.StatusPage.Providers.StatusProvider` klassi, näiteks:

```csharp
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;
using Nest;

namespace My.Web
{
    public class ElasticStatusProvider : StatusProvider
    {
        private readonly string connectionString;
        private readonly string index;

        public ElasticStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            connectionString = configuration.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(@"Elasticsearch ühenduse parameetrid on konfiguratioonifailis määramata.", nameof(connectionString));

            configuration.UnrecognizedAttributes.TryGetValue("index", out string indexValue);
            index = indexValue.GetAppSettingsOrValue();
            if (string.IsNullOrWhiteSpace(index))
                throw new ArgumentException(@"Elasticsearch indeksi nimi on konfiguratioonifailis määramata.", nameof(index));
        }

        protected override string GetUri() => $"{index}@{connectionString}";

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            externalUnit.ServerPlatform = new ServerPlatform { Name = "Elasticsearch" };

            using (var c = new WebClient())
            {
                var data = c.DownloadString(connectionString);
                var json = JObject.Parse(data);
                var version = json.SelectToken("version.number");

                externalUnit.ServerPlatform.Version = version != null ? (string)version : null;
            }

            var client = new ElasticClient(new ConnectionSettings(new Uri(connectionString)).DefaultIndex(index).MaximumRetries(3));
            var response = client.ClusterHealth(x => x.Index(index));

            if (!response.Status.Equals("green") && !response.Status.Equals("yellow"))
                return externalUnit.SetStatus(UnitStatus.NotOk, "Elasticsearch ei tööta.");

            return externalUnit.SetStatus(UnitStatus.Ok);
        }
    }
}
```

### SinuEndaKlots

Kui sulle tundub, et oled leiutanud väärt *provider*-i, millest võiks ka teistele kasu tõusta, siis jaga oma
loomingut *Pull Request*-iga.
