﻿import helpers from "./../../Core/Content/scripts/Helpers";
import fileUploadController from "./../../Core/Controls/FileUpload/file-upload";
import confirm from "./../../Core/Controls/Confirm/Confirm";
import alertify from 'alertifyjs/build/alertify.min';

let holder;
let descriptionElem;
let editor;

function initEditor() {
    descriptionElem = holder.querySelector(".js-edit-bulletin__description");
    let dataStorage = holder.querySelector(".js-edit-bulletin__description-hidden");

    editor = helpers.initQuill(descriptionElem, dataStorage, {
        theme: 'snow',
        modules: {
            toolbar: [
                ['bold', 'italic', 'link'],
                ['emoji']
            ]
        }
    });
    let emojiContainer = editor.container.querySelector(".js-emoji");
    if (!emojiContainer) {
        helpers.initSmiles(editor, editor.getModule('toolbar').container);
        emojiContainer = true;
    }

    editor.on('text-change', function () {
        if (editor.getLength() > 1 && descriptionElem.classList.contains('input-validation-error')) {
            descriptionElem.classList.remove('input-validation-error');
        }
    });
}

function initFileUploader() {
    fileUploadController.init(holder);
}

function initEventListeners() {
    let deleteButton = holder.querySelector(".js-edit-bulletin__delete");
    deleteButton.addEventListener("click", deleteClickHandler);

    let submitButton = holder.querySelector(".js-disable-submit");
    submitButton.addEventListener("click", submitClickHandler);
}

function submitClickHandler(event) {
    $(descriptionElem).toggleClass("input-validation-error", editor.getLength() <= 1);

    if (!isModelValid()) {
        holder.querySelector('.form__required').style.display = 'block';
        event.preventDefault();
        event.stopPropagation();
    }
}

function isModelValid() {
    let decription = holder.querySelector('input[name="description"]').value;
    let media = holder.querySelector('input[name="newMedia"]').value;
    let newMedia = holder.querySelector('input[name="media"]').value;
    return decription || media || newMedia ? true : false;
}

function deleteClickHandler(event) {
    let button = event.target;
    let id = button.dataset["id"];
    let text = button.dataset["text"];
    let returnUrl = button.dataset["returnUrl"];
    let deleteUrl = button.dataset["deleteUrl"];

    alertify.defaults.glossary.cancel = button.dataset["cancel"];
    alertify.defaults.glossary.ok = button.dataset["ok"];

    confirm.showConfirm('', text, function () {
        $.post(deleteUrl + '?id=' + id, function (data) {
            if (data.IsSuccess) {
                window.location.href = returnUrl;
            }
        });
    }, function () { }, confirm.defaultSettings);

    return false;
}

function getBulletinHolder() {
    return document.querySelector(".js-edit-bulletin");
}

let controller = {
    init: function () {
        holder = getBulletinHolder();
        if (!holder) {
            return;
        }

        initEditor();
        initFileUploader();
        initEventListeners();
    }
}

export default controller;