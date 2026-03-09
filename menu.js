// menu.js

// Quando a página carregar:
window.addEventListener('DOMContentLoaded', () => {
  const linkMateriais = document.getElementById('linkMateriais');
  const linkLogout = document.getElementById('linkLogout');

  // Supondo que o login gere um token salvo no localStorage:
  const token = localStorage.getItem('token'); 
  
  if (token) {
    // Usuário está logado
    linkMateriais.style.display = 'inline-block';
    linkLogout.style.display = 'inline-block';
    linkLogout.addEventListener('click', (e) => {
      e.preventDefault();
      localStorage.removeItem('token'); // Remove o token
      window.location.href = 'login.html';
    });
  } else {
    // Usuário NÃO logado
    linkMateriais.style.display = 'none';
    linkLogout.style.display = 'none';
  }
});
