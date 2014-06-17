<%@ Page Language="C#" %>
<%@ OutputCache VaryByParam="none" Duration="360000" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        //if (!Request.IsLocal && !Request.Url.Authority.StartsWith("m82.be"))
        //    Response.Redirect("http://1.m82.be/", true);
    }

</script>

<!doctype html>
<html>
<head>
    <title>Reverse Proxy CDN</title>
    <meta charset="utf-8" />
    <style>
        body {
            font: 1em/1 arial;
            max-width: 960px;
            margin: 1em auto;
        }

        p {
            font-weight: bold;
        }

            p span {
                font-weight: normal;
            }
    </style>
</head>
<body>

    <h1>Reverse Proxy CDN</h1>
    <p>Location: <span><%= ConfigurationManager.AppSettings.Get("location") %></span></p>
    <%=Request.Url.Authority %>
</body>
</html>