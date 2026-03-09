const apiUrl = "http://localhost:5008/api/material"; // URL correta da sua API

// Valida o loginForm antes de adicionar listener
const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const email = document.getElementById("email").value;
        const senha = document.getElementById("senha").value;

        const response = await fetch("http://localhost:5008/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, senhaHash: senha })
        });

        if (response.ok) {
            alert("Login bem-sucedido!");
            window.location.href = "index.html"; // Redireciona
        } else {
            alert("Login inválido.");
        }
    });
}

// Notificações
function showNotification(message) {
    const notification = document.getElementById("notification");
    notification.textContent = message;
    notification.classList.remove("hidden");

    setTimeout(() => notification.classList.add("hidden"), 3000);
}

// Confirmação
let confirmCallback = null;

function showConfirm(message, callback) {
    const confirmBox = document.getElementById("confirm-box");
    document.getElementById("confirm-message").textContent = message;
    confirmCallback = callback;
    confirmBox.classList.remove("hidden");
    confirmBox.classList.add('show');
}

function confirmAction(confirmed) {
    if (confirmCallback) confirmCallback(confirmed);
    document.getElementById("confirm-box").classList.add("hidden");
}

// Cadastro
async function cadastrarMaterial() {
    const nome = document.getElementById("nome").value;
    const descricao = document.getElementById("descricao").value;
    const quantidade = document.getElementById("quantidade").value;

    if (!nome || !descricao || isNaN(quantidade) || quantidade <= 0) {
        return showNotification("Preencha todos os campos corretamente.");
    }

    const material = { nome, descricao, quantidade: parseInt(quantidade) };
    try {
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(material)
        });

        if (response.ok) {
            showNotification("Material cadastrado com sucesso!");
            document.getElementById("nome").value = "";
            document.getElementById("descricao").value = "";
            document.getElementById("quantidade").value = "";
            listarMateriais();

            const novoMaterial = await response.json();
            const usuarioId = localStorage.getItem('usuarioId'); // recupera o id do usuário logado

            // 📍 Registra a movimentação tipo "cadastro"
            await fetch(`${apiUrl}/registrar-movimentacao`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    materialId: novoMaterial.id,
                    quantidade: parseInt(quantidade),
                    tipo: "cadastro",
                    usuarioId: parseInt(usuarioId)
                })
            });

        } else if (response.status === 409) {
            showNotification("Material já cadastrado com esse nome.");
        } else {
            showNotification(`Erro ao cadastrar material: ${response.statusText}`);
        }
    } catch (error) {
        console.error(error);
        showNotification("Erro ao cadastrar material.");
    }

            /*const usuarioId = localStorage.getItem('usuarioId'); // recupera o id do usuário logado
            await fetch(`${apiUrl}/registrar-movimentacao`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    materialId: (await response.json()).id, // se a API retornar o ID gerado
                    quantidade: parseInt(quantidade),
                    tipo: "cadastro",
                    usuarioId: parseInt(usuarioId)
                })
            });


        } else {
            showNotification(`Erro ao cadastrar material: ${response.status}`);
        }
        if (response.status === 409) {
            showNotification("Material já cadastrado com esse nome.");
        } else if (!response.ok) {
            showNotification(`Erro ao cadastrar material: ${response.statusText}`);
        }
    } catch (error) {
        console.error(error);
        showNotification("Erro ao cadastrar material.");
    }*/
}

// Listagem
async function listarMateriais() {
    const tabela = document.getElementById("materiais-list");
    if (!tabela) return;

    try {
        const response = await fetch(apiUrl);
        if (!response.ok) throw new Error(`Erro ${response.status}`);

        const materiais = await response.json();
        tabela.innerHTML = ""; // limpa a lista
        materiais.forEach(material => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${material.nome}</td>
                <td>${material.descricao}</td>
                <td>${material.quantidade}</td>
                <td style="white-space:nowrap;">
                    <input type="number" id="qtd-${material.id}" min="1" placeholder="Qtd" style="width:60px;" />
                    <button class="btn-add" onclick="adicionarMaterial(${material.id})">➕</button>
                    <button class="btn-remove" onclick="excluirMaterial(${material.id})">➖</button>
                    <button onclick="excluirMaterialCompleto(${material.id})">🗑️</button>
                </td>
            `;
            tabela.appendChild(row);
        });
    } catch (error) {
        console.error(error);
        showNotification(`Erro ao listar materiais.`);
    }
}

// Excluir parcial
async function excluirMaterial(id) {
    const input = document.getElementById(`qtd-${id}`);
    const qtdExcluir = parseInt(input.value, 10);

    if (isNaN(qtdExcluir) || qtdExcluir <= 0) {
        showNotification("Informe uma quantidade válida para excluir.");
        return;
    }

    showConfirm(`Tem certeza que deseja excluir ${qtdExcluir} unidades?`, async (confirmed) => {
        if (!confirmed) return;

        try {
            const response = await fetch(`${apiUrl}/${id}/remover`, {
                method: 'PUT',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ quantidade: qtdExcluir })
            });

            if (response.ok) {
                showNotification(`Removido ${qtdExcluir} unidade(s) com sucesso!`);

                const usuarioId = localStorage.getItem('usuarioId'); // id do usuário logado
                await fetch(`${apiUrl}/registrar-movimentacao`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ materialId: id, quantidade: qtdExcluir, tipo: "saida", usuarioId: parseInt(usuarioId) })
                });

                listarMateriais();
            } else {
                showNotification(`Erro ao remover material: ${response.statusText}`);
            }
        } catch (error) {
            console.error(error);
            showNotification(`Erro ao remover material.`);
        }
    });
}

// Adicionar
async function adicionarMaterial(id) {
    const input = document.getElementById(`qtd-${id}`);
    const qtdAdd = parseInt(input.value, 10);

    if (isNaN(qtdAdd) || qtdAdd <= 0) {
        showNotification("Informe uma quantidade válida para adicionar.");
        return;
    }

    try {
        const response = await fetch(`${apiUrl}/${id}/adicionar`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ quantidade: qtdAdd })
        });

        if (response.ok) {
            showNotification(`Adicionado ${qtdAdd} unidade(s)!`);

            const usuarioId = localStorage.getItem('usuarioId'); // id do usuário logado
            await fetch(`${apiUrl}/registrar-movimentacao`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ materialId: id, quantidade: qtdAdd, tipo: "entrada", usuarioId: parseInt(usuarioId) })
            });

            listarMateriais();
        } else {
            showNotification(`Erro ao adicionar material: ${response.statusText}`);
        }
    } catch (error) {
        console.error(error);
        showNotification(`Erro ao adicionar material.`);
    }
}

// Excluir completo
async function excluirMaterialCompleto(id) {
    showConfirm(`Deseja excluir o material completamente da lista?`, async (confirmed) => {
        if (!confirmed) return;

        try {
            const row = document.getElementById(`qtd-${id}`).closest('tr');
            const qtdAtual = parseInt(row.children[2].textContent.trim()) || 0;

            const response = await fetch(`${apiUrl}/${id}`, { method: "DELETE" });

            if (response.ok) {
                showNotification("Material excluído completamente com sucesso!");

                if (qtdAtual > 0) {
                    const usuarioId = localStorage.getItem('usuarioId'); // id do usuário logado
                    await fetch(`${apiUrl}/registrar-movimentacao`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ materialId: id, quantidade: qtdAtual, tipo: "saida", usuarioId: parseInt(usuarioId) })
                    });
                }
                listarMateriais();
            } else if (response.status === 404) {
                showNotification("Material não encontrado.");
            } else {
                showNotification(`Erro ao excluir material: ${response.statusText}`);
            }
        } catch (error) {
            console.error(error);
            showNotification(`Erro ao excluir material.`);
        }
    });
}

// Chama a listagem apenas na página que tem o id materiais-list
window.onload = () => {
    if (document.getElementById("materiais-list")) {
        listarMateriais();
    }
};
