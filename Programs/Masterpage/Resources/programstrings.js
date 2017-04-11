// Add translated strings here
pgms_strings = {
    "connect": [
        "Programs",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "zhhk": [
        "方案",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "zhcn": [
        "方案",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "fr": [
        "Programmes",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "de": [
        "Programme",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Seiten",
        "Featured Files"
    ],
    "ja": [
        "Programs",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "pl": [
        "Programy",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Strony",
        "Featured Files"
    ],
    "pt": [
        "Programas",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Paginas",
        "Featured Files"
    ],
    "ru": [
        "Программы",
        "Active Programs",
        "Add New Program",
        "See all documents",
        "Edit",
        "Run Time",
        "Pages",
        "Featured Files"
    ],
    "es": [
        "Programas",
        "Programas Activos",
        "Agregar Nuevo Programa",
        "Ver todos los documentos",
        "Editar",
        "Tiempo de ejecución",
        "Paginas",
        "Archivos recomendados"
    ]
}

function GetProgramString(textToTranslate, targetLanguage) {
    return pgms_strings[targetLanguage][pgms_strings["connect"].indexOf(textToTranslate)];
}

function GetSiteLang() {
    var url = window.location.href;
    var np = url.substring(url.indexOf(":") + 3, url.length);
    var ns = np.substring(np.indexOf("/") + 1, np.length);
    var lang = "";
    if (ns.indexOf("/") == -1) {
        lang = ns.replace("#", "").replace("?", "");
    }
    else {
        lang = ns.substring(0, ns.indexOf("/"));
    }
    return lang.toLowerCase();
}

function GetSiteLanguage() {
    var lang = GetSiteLang();
    switch (lang.toLowerCase()) {
        case "connect":
            return "English";
        case "zhhk":
            return "Chinese (Hong Kong S.A.R.)";
        case "zhcn":
            return "Chinese (People's Republic of China)";
        case "fr":
            return "French (France)";
        case "de":
            return "German (Germany)";
        case "ja":
            return "Japanese (Japan)";
        case "pl":
            return "Polish (Poland)";
        case "pt":
            return "Portuguese (Brazil)";
        case "ru":
            return "Russian (Russia)";
        case "es":
            return "Spanish (Spain)";
        default:
            return "English";
    }
}

function ConfirmActive(runTime, startDate, endDate) {
    var active = false;
    var today = new Date();
    var start = new Date(startDate);
    var end = new Date(endDate);
    if (runTime == "Fixed") {
        if (today >= start && today < end) {
            active = true;
        }
    }
    else if (runTime == "Open") {
        if (today >= start) {
            active = true;
        }
    }
    return active;
}