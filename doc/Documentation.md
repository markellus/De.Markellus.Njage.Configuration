# NJAGE Configuration API - Dokumentation

## Aufbau einer Konfigurationsdatei

```xml
<?xml version="1.0" encoding="utf-8"?>
<njageconfig version="1.1">
    <setting1 value="a string value" type="string"/>
    <somesortofrectangle>
        <x value="100" type="int"/>
        <y value="100" type="int"/>
        <width value="1280" type="uint"/>
        <height value="720" type="uint"/>
    </somesortofrectangle>
</njageconfig>
```

* Eine Konfigurationsdatei beinhaltet als allererstes immer den XML-Header : 
  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  ```
* Der oberste XML-Knoten lautet immer **njageconfig** und besitzt ein Attribut **version** mit der verwendeten Version des Dateiformats.
* Die aktuelle Version des Dateiformates ist **1.1**

## Regeln für Attributnamen

* Namen dürfen nicht mit zwei Unterstrichen (__) beginnen

## Typen von Konfigurationsknoten

### Einfacher Konfigurationsknoten

Ein einfacher Konfigurationsknoten besitzt mindestens ein **type**-Attribut, welches den Typ der enthaltenen Information festlegt. Alle weiteren Attribute und ggf. Unterknoten sind vom Tp der Konfiguration abhängig.
* Beispiel für einen einfachen Konfigurationsknoten, der eine Zeichenkette mit dem Namen "info" speichert:
  ```xml
  <info value="a string value" type="string"/>
  ```
  
### Listenkonfigurationsknoten

Ein Listenkonfigurationsknoten besitzt mehrere Unterknoten vom selben Typ, die beispielsweise als Array oder Liste interpretiert werden können. Die Unterknoten können selbst Listenkonfigurationsknoten sein, sodass eine beliebige Verschachtelung möglich ist. Der Typ des Listenkonfigurationsknoten lautet **list** und er besitzt zusätzlich ein Attribut **content**, welches den Typ der Unterknoten angibt.
Alle Unterknoten müssen den Namen **Item** haben.

* Beispiel für einen Listenkonfigurationsknoten mit einfachen Konfigurationsknoten vom Typ string als Unterknoten:
  ```xml
  <infolist content="string" type="list">
	  <item value="first index" type="string"/>
	  <item value="second index" type="string"/>
	  <item value="third index" type="string"/>
  </infolist>
  ```
  
* Beispiel für eine verschachtelte Listenkonfigurationsknoten:
  ```xml
  <infolist content="list" type="list">
	  <item content="string" type="list">
		  <item value="first index" type="string"/>
		  <item value="second index" type="string"/>
		  <item value="third index" type="string"/>
	  </item>
	  <item content="string" type="list">
		  <item value="first index" type="string"/>
		  <item value="second index" type="string"/>
		  <item value="third index" type="string"/>
	  </item>
  </infolist>
  ```

### Alle einfachen Tpen

#### string
