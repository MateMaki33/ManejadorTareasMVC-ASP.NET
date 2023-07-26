﻿function agregarNuevaTareaAlListedo() {
    tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel({ id: 0, titulo: '' }));

    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusOutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListadoViewModel.tareas.pop();
        return;
    }

    const data = JSON.stringify(titulo);
    const respuesta = await fetch(urlTareas, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (respuesta.ok) {
        const json = await respuesta.json();
        tarea.id(json.id);
    } else {
        manejarErrorApi(respuesta)
    }
}

async function obtenerTareas() {
    tareaListadoViewModel.cargando(true);

    const respuesta = await fetch(urlTareas, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);
    json.forEach(valor => {
        tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel(valor));
    });
    tareaListadoViewModel.cargando(false);
}

/*........................... REORDENACION DE TAREAS ARRASTRANDO............................*/

// 1. Obtenemos el Id de las tareas desde la vista con Jquery
// 2. Enviamos al backend el array de ids para ordenar la base de datos
// 3. Una vez reordenada la base de datos, ordenamos el array en memoria para mostrar

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasAlBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);
}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get(); //con get lo convertimos a arreglo de Ids
    return ids;
}

async function enviarIdsTareasAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTareas}/ordenar`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

async function manejarClickTarea(tarea) {
    if (tarea.esNuevo()) {
        return;
    }

    const respuesta = await fetch(`${urlTareas}/${tarea.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }
    const json = await respuesta.json();
    tareaEditarVM.id = json.id;
    tareaEditarVM.titulo(json.titulo);
    tareaEditarVM.descripcion(json.descripcion);
    tareaEditarVM.pasos([]);
    json.pasos.forEach(paso => {
        tareaEditarVM.pasos.push(new pasoViewModel({...paso,modoEdicion : false}))
    })

    prepararArchivosAdjuntos(json.archivosAdjuntos);
    modalEditarTareaBootstrap.show();
}

async function manejarCambioEditarTarea() {
    const obj = {
        id: tareaEditarVM.id,
        titulo: tareaEditarVM.titulo(),
        descripcion: tareaEditarVM.descripcion()
    };
    if(!obj.titulo){
        return;
    }

    await editarTareaCompleta(obj);
    const indice = tareaListadoViewModel.tareas().findIndex(t => t.id() === obj.id);
    const tarea = tareaListadoViewModel.tareas()[indice];
    tarea.titulo(obj.titulo);
}

async function editarTareaCompleta(tarea) {
    const data = JSON.stringify(tarea);
    const respuesta = await fetch(`${urlTareas}/${tarea.id()}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    }); 
    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        throw "error";
    }
}

function intentarBorrarTarea(tarea) {
    modalEditarTareaBootstrap.hide();
    confirmarAccion({
        callbackAceptar: () => {
            borrarTarea(tarea);
        },
        callbackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `¿Desea borrar la tarea ${tarea.titulo()}?`
    })
}

async function borrarTarea(tarea) {
    const idTarea = tarea.id;
    const respuesta = await fetch(`${urlTareas}/${idTarea}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (respuesta.ok) {
        const indice = obtenerIndiceTareaEnEdicion();
        tareaListadoViewModel.tareas.splice(indice, 1);
    }
}

function obtenerIndiceTareaEnEdicion() {
    return tareaListadoViewModel.tareas().findIndex(t => t.id() == tareaEditarVM.id);
}

function obtenerTareaEnEdicion() {
    const indice = obtenerIndiceTareaEnEdicion();
    return tareaListadoViewModel.tareas()[indice];
}


//Al arrastrar se ejecutan las funciones
$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    })
})

/*..................................FIN REORDENACION DE TAREAS............................................................... */

