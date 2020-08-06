
function AddNewVersion(page) {
    var pageValues = "";
    var url = "";
    if (page == "edit") {
        pageValues = $('#editFile').find('select, textarea, input').serialize();
        url = "Edit?handler=AddVersion";
    }
    if (page == "add") {
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
        url = "Add?handler=AddVersion";
    }
    console.log(pageValues);
    $.ajax({
        async: true,
        data: pageValues,
        type: "POST",
        url: url,
        success: function (partialView) {
            console.log(partialView)
            $('#pluginVersionContainer').html(partialView);
        }
    })
}

function SavePlugin() {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "Edit?handler=SavePlugin",
        success: function (modalPartialView) {
            console.log(modalPartialView);
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function AddPlugin() {
    pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "Add?handler=SavePlugin",
        success: function (modalPartialView) {
            console.log(modalPartialView);
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function ShowNewPluginModal() {
    var placeholderElement = $('#addModalContainer');

    $.ajax({
        type: "GET",
        url: "ConfigTool?handler=AddPlugin",
        success: function (modalPartialView) {
            console.log(modalPartialView);
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function ShowConfirmationModal(id,name) {
    var placeholderElement = $('#modalContainer');
    document.getElementById("selectedPluginId").value = id;
    document.getElementById("selectedPluginName").value = name;

    pageValues = $('#configToolPage').find('input').serialize();
    console.log(pageValues);
    $.ajax({
        data: pageValues,
        type: "GET",
        url: "ConfigTool?handler=ShowDeleteModal",
        success: function (modalPartialView) {
            console.log(modalPartialView);
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function RedirectToList() {
    var url = `${window.location.origin}/configtool`;
    window.location.href = url;
}

function ShowVersionDetails(versionId) {
    var pageValues = "";
    var pageName = location.pathname.split("/").slice(-1).toString().toLowerCase();
    var url = `${pageName}?handler=ShowVersionDetails`;

    console.log(pageName);
    document.getElementById("selectedVersionId").value = versionId;

    if (pageName == "edit") {
        pageValues = $('#editFile').find('select, textarea, input').serialize();
    }
    if (pageName == "add") {
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    }

    $.ajax({
        async: true,
        data: pageValues,
        cache: false,
        type: "POST",
        url: url,
        success: function (partialView) {
            console.log(partialView)
            $('#pluginVersionContainer').html(partialView);
        }
    })
}

