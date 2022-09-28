var users = new Map();

const grid = new gridjs.Grid({
    columns: [
        {
            name: gridjs.html(`<input type="checkbox" onclick="selectAllUsers(this)"/>`),
            id: 1,
            formatter: (cell, row) => gridjs.html(`<input type="checkbox" id="checkbox-${row.cells[1].data}" onclick="selectUser('${row.cells[1].data}')"/>`)
        },
        'Id',
        'Name',
        'E-mail',
        'Last login time',
        'Registration time',
        {
            name: 'Status',
            id: 7,
            formatter: cell => gridjs.html(`<span class="fw-bold ${cell ? 'text-danger' : 'text-success'}">${cell ? 'Blocked' : 'Active'}</span>`)
        }
    ],
    data: () => {
        return fetchUsers().then(() =>
            Array.from(users.values()).map(user => ['', user.id, user.name, user.email, user.lastLogin, user.created, user.blocked])
        )
    },
    style:{
        table: {
            width: '100%'
        },
        td: {
            'overflow-wrap': 'normal',
        }
    },
    className: {
        table: 'table table-hover',
        thead: 'table-dark'
    }
}).render($('#tablewrapper')[0]);

function fetchUsers() {
    return fetch("getusers").then((r) =>
        r.json().then(fetchedUsers => users = new Map(
            fetchedUsers.map(user => [user.id, { selected: false, ...user }])
        ))
    );
}

function updateTable() {
    grid.updateConfig({
        data: Array.from(users.values()).map(user => ['', user.id, user.name, user.email, user.lastLogin, user.created, user.blocked])
    }).forceRender();
}

function selectUser(id, value = null) {
    let user = users.get(id);
    user.selected = value || !user.selected;
    $('#checkbox-' + id)[0].checked = user.selected;
}

function selectAllUsers(checkbox) {
    for (let id of users.keys()) selectUser(id, checkbox.checked);
}

function modifyUsers(action, params = null) {
    let ids = Array.from(users.values()).filter(user => user.selected).map(user => user.id);
    let url = action;
    if (params != null) {
        url += '?';
        for (let key in params) url += `${key}=${params[key]}&`;
    }
    return fetch(url, {
        method: 'POST',
        body: JSON.stringify(ids),
        redirect: 'manual',
        headers: { 'content-type': 'application/json' }
    }).then(r => {
        if (r.status == 0) {
            window.location.replace('/Account/Login');
            return null;
        }
        if (!r.ok) {
            alert(`There was an issue connecting to the server. Status: ${r.status} ${r.statusText}.`);
            return null;
        }
        return r.json();
    }).catch((error) => {
        alert(`There was an issue connecting to the server (${error}).`);
        return null;
    });
}

function deleteUsers() {
    return modifyUsers("deleteusers").then(deletedIds => {
        deletedIds.forEach(id => {
            users.delete(id);
        });
        updateTable();
    });
}

function setBlockStatus(status) {
    return modifyUsers("setblockstatus", {status: status}).then(setIds => {
        setIds.forEach(id => {
            users.get(id).blocked = status;
            selectUser(id, false);
        });
        updateTable();
    });
}