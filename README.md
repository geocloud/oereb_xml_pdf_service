<b>Kurzüberblick ÖREB XML2PDF Service</b>

Der ÖREB PDF Service wandelt einen validen ÖREB DATA-Extract (XML) einer ÖREB-Auswertung in ein konformes PDF-Dokument um. 

<b>Randbedingungen</b>

Die folgenden Dokumente geben die technischen Randbedingungen des Projektes vor:

<ul>
<li>Weisung vom 1. November 2017 (Stand am 25. August 2017), ÖREB-Kataster DATA-Extract Schemaversion 1.03</li>
<li>Weisung vom 1. Juli 2015 (Stand am 1. Juli 2015), ÖREB-Kataster Inhalt und Darstellung des statischen Auszugs Vorlage ÖREB-Katasterauszug: Satzanweisungen und Vermassung</li>
</ul>

<b>REST Schnittstelle</b>

Die Daten können auch direkt gesendet werden z.B. im Zuge einer Server-Server Kommunikation.

https://oereb-dev.gis-daten.ch/oereb/report/form

Es muss eine "POST" Anweisung an diese Adresse geschickt werden im Format "multipart/form-data". Es müssen folgende Variablen abgefüllt sein:

<table>
<tr><td>file</td><td></td><td>Byte Array</td></tr>
<tr><td>token</td><td></td><td>Ein Zugriffstoken welche Ihnen Zugang gewährt, Bsp. 1c7751fc-1379-49df-98e3-540f09b11dcb (string)</td></tr>
<tr><td>validate</td><td>true</td><td>vor der Berichtsaufbereitung wird das Dokument noch validiert und falls ein Validierungsfehler auftritt die Aufbereitung gestoppt.</td></tr>
<tr><td></td><td>false</td><td>Es wird keine Validierung durchgeführt, es kann aber sein das dadurch die Berichtaufbereitung mit einem Fehler abbricht</td></tr>
<tr><td>usewms</td><td>true</td><td>benutzen der WMS Referenzen, ein gültiger Link wird vorausgesetzt</td></tr>
<tr><td></td><td>false</td><td>es wird davon ausgegangen, dass die Bilder eingebettet sind</td></tr>
<tr><td>flavour</td><td>reduced</td><td>Anhänge werden nicht abgefragt und angehängt</td></tr>
<tr><td></td><td>complete</td><td>sind die verlinkten Dokumente als PDF vorhanden werden diese abgefragt, in ein Bild umgewandelt und im Bericht als weitere Seiten angehängt </td></tr>
<tr><td></td><td>completeAttached</td><td>sind die verlinkten Dokumente als PDF vorhanden werden diese abgefragt und im PDF als Anhang integriert.</td></tr>
</table>
	

<b>Fehlermeldungen</b>

Im besten Fall wird der Status Http.OK (200) zurückgegeben und als Mime-Typ "application/pdf".

Treten Probleme auf, so wird der Status Http.BadRequest im Json-Format zurückgegeben. Mit den Eigenschaften Status und Messages. Bei Messages werden die Validierungsprobleme angeführt.

Beispiel:
{
	"Status":"Submitted token is not valid or does not exist"
}
