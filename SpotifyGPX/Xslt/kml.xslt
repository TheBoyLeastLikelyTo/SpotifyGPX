<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:kml="http://www.opengis.net/kml/2.2" exclude-result-prefixes="kml">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/">
        <html>
            <head>
                <title>Pairs Table - <xsl:value-of select="kml:kml/kml:Document/kml:name" /></title>
                <link rel="stylesheet" href="styles.css" />
            </head>
            <body>
                <h1>Pairs Table - <xsl:value-of select="//kml:Document/kml:name"/></h1>
                <hr />
                <p>Name: <xsl:value-of select="//kml:Document/kml:name" /></p>
                <p>Hash: <xsl:value-of select="//kml:Document/kml:description" /></p>
                <table>
                    <tr>
                        <th>#</th>
                        <th>Latitude</th>
                        <th>Longitude</th>
                        <th>Name</th>
                        <th>Time</th>
                        <th>Description</th>
                    </tr>
                    <xsl:apply-templates select="//kml:Placemark"/>
                </table>
                <hr />
            </body>
        </html>
    </xsl:template>
    
    <!-- Match each placemark and populate the table row -->
    <xsl:template match="kml:Placemark">
        <xsl:variable name="coordinates" select="kml:Point/kml:coordinates"/>
        <xsl:variable name="lon" select="substring-before($coordinates, ',')"/>
        <xsl:variable name="lat" select="substring-after($coordinates, ',')"/>
        <xsl:variable name="time" select="kml:TimeStamp/kml:when"/>
        <tr>
            <td><xsl:number value="position()" format="1"/></td>
            <td><xsl:value-of select="$lat"/></td>
            <td><xsl:value-of select="$lon"/></td>
            <td><xsl:value-of select="kml:name"/></td>
            <td><xsl:value-of select="$time"/></td>
            <td><xsl:apply-templates select="kml:description"/></td>
        </tr>
    </xsl:template>

    <!-- Template to match the <description> tag -->
    <xsl:template match="kml:description">
        <xsl:call-template name="replace-newline">
            <xsl:with-param name="text" select="."/>
        </xsl:call-template>
    </xsl:template>

    <!-- Template to recursively replace CRLF with <br> -->
    <xsl:template name="replace-newline">
        <xsl:param name="text"/>
        <xsl:choose>
            <xsl:when test="contains($text, '&#xA;')">
                <xsl:value-of select="substring-before($text, '&#xA;')"/>
                <br/>
                <xsl:call-template name="replace-newline">
                    <xsl:with-param name="text" select="substring-after($text, '&#xA;')"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$text"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
