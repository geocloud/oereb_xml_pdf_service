Kurzüberblick ÖREB XML2PDF Service 

Der ÖREB PDF Service wandelt einen validen ÖREB DATA-Extract (XML) einer ÖREB-Auswertung in ein konformes PDF-Dokument um. 

Randbedingungen

Die folgenden Dokumente geben die technischen Randbedingungen des Projektes vor:

•	Weisung vom 1. November 2017 (Stand am 25. August 2017), ÖREB-Kataster DATA-Extract Schemaversion 1.03
•	Weisung vom 1. Juli 2015 (Stand am 1. Juli 2015), ÖREB-Kataster Inhalt und Darstellung des statischen Auszugs Vorlage ÖREB-Katasterauszug: Satzanweisungen und Vermassung

REST Schnittstelle

Die Daten können auch direkt gesendet werden z.B. im Zuge einer Server-Server Kommunikation.

https://oereb-dev.gis-daten.ch/oereb/report/form

Es muss eine "POST" Anweisung an diese Adresse geschickt werden im Format "multipart/form-data". Es müssen folgende Variablen abgefüllt sein:

file	Byte Array

token		Ein Zugriffstoken welche Ihnen Zugang gewährt, 
			Bsp. 1c7751fc-1379-49df-98e3-540f09b11dcb (string)
		
validate	true	vor der Berichtsaufbereitung wird das Dokument noch validiert und falls ein Validierungsfehler auftritt die Aufbereitung gestoppt.
			false	Es wird keine Validierung durchgeführt, es kann aber sein das dadurch die Berichtaufbereitung mit einem Fehler abbricht
			
usewms		true	benutzen der WMS Referenzen, ein gültiger Link wird vorausgesetzt
			false	es wird davon ausgegangen, dass die Bilder eingebettet sind

flavour		reduced	
			Anhänge werden nicht abgefragt und angehängt

			complete
			sind die verlinkten Dokumente als PDF vorhanden werden diese abgefragt, in ein Bild umgewandelt und im Bericht als weitere Seiten angehängt 
			
			completeAttached
			sind die verlinkten Dokumente als PDF vorhanden werden diese abgefragt und im PDF als Anhang integriert.

Fehlermeldungen

Im besten Fall wird der Status Http.OK (200) zurückgegeben und als Mime-Typ "application/pdf".

Treten Probleme auf, so wird der Status Http.BadRequest im Json-Format zurückgegeben. Mit den Eigenschaften Status und Messages. Bei Messages werden die Validierungsprobleme angeführt.

Beispiel:
{
	"Status":"Submitted token is not valid or does not exist"
}
