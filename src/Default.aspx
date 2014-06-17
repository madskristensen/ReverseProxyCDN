<%@ Page Language="C#" %>
<%@ OutputCache VaryByParam="none" Duration="360000" %>

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

        a{
            position: absolute;
            top: 0;
            right: 0;
        }
    </style>
    <script>
        var host = location.host;

        if (host.indexOf("m82.be") === 0) {
            location.href = "http://1.m82.be";
        }
    </script>
</head>
<body>

    <h1>Reverse Proxy CDN</h1>
    <p>Location: <span><%= ConfigurationManager.AppSettings.Get("location") %></span></p>

    <a href="https://github.com/madskristensen/ReverseProxyCDN/">
        <img src="http://schemastore.org/img/forkme.png" alt="Fork me on GitHub" />
    </a>

</body>
</html>