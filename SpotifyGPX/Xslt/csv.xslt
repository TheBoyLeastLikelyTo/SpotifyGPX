<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    
    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <!-- Match the root element and start creating HTML structure -->
    <xsl:template match="/Root">
        <html>
            <head>
                <title>Pairs Table</title>
                <link rel="stylesheet" href="styles.css"/>
            </head>
            <body>
                <h1>Pairs Table</h1>
                <hr/>
                <table>
                    <xsl:apply-templates select="Line"/>
                </table>
                <hr/>
            </body>
        </html>
    </xsl:template>

    <!-- Template for processing each Line element -->
    <xsl:template match="Line">
        <tr>
            <xsl:call-template name="tokenize">
                <xsl:with-param name="text" select="normalize-space(.)"/>
            </xsl:call-template>
        </tr>
    </xsl:template>

    <!-- Template for tokenizing a comma-separated string -->
    <xsl:template name="tokenize">
        <xsl:param name="text"/>
        <xsl:param name="separator" select="','"/>
        <xsl:choose>
            <xsl:when test="contains($text, $separator)">
                <td>
                    <xsl:value-of select="substring-before(concat($text, $separator), $separator)"/>
                </td>
                <xsl:call-template name="tokenize">
                    <xsl:with-param name="text" select="substring-after($text, $separator)"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <td>
                    <xsl:value-of select="normalize-space($text)"/>
                </td>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
