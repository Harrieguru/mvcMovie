// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById('addDirector').addEventListener('click', function (e) {
    e.preventDefault();

    let div = document.createElement('div');
    div.className = 'input-group mb-2';

    let input = document.createElement('input');
    input.type = 'text';
    input.className = 'form-control';
    input.name = 'Directors';
    div.appendChild(input);

    let inputGroupAppend = document.createElement('div');
    inputGroupAppend.className = 'input-group-append';

    let removeButton = document.createElement('button');
    removeButton.className = 'btn btn-outline-danger';
    removeButton.textContent = '-';
    removeButton.addEventListener('click', function () {
        div.remove();
    });
    inputGroupAppend.appendChild(removeButton);

    div.appendChild(inputGroupAppend);

    document.getElementById('directorGroup').appendChild(div);
});