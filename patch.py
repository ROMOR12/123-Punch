import os

file_path = r'c:\Users\shark\Desktop\123-Punch\Assets\_Game\Scripts\UI\Views\MenuPrincipal\GameManager.cs'

with open(file_path, 'r', encoding='latin-1') as f:
    content = f.read()

injection = """
    private bool notificacionesInicializadas = false;
    private void Update()
    {
        if (!notificacionesInicializadas && SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            new NotificacionesService().Inicializar();
            notificacionesInicializadas = true;
        }
    }

"""

content = content.replace("private void Awake()", injection + "    private void Awake()")

with open(file_path, 'w', encoding='latin-1') as f:
    f.write(content)

print("Patch applied to GameManager.cs")
