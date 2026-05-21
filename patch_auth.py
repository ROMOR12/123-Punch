import os

file_path = r'c:\Users\shark\Desktop\123-Punch\Assets\_Game\Scripts\Services\Auth\AuthManager.cs'

with open(file_path, 'r', encoding='latin-1') as f:
    content = f.read()

target = """        FirebaseUser user = auth.CurrentUser;
        if (user.IsEmailVerified)
        {
            // Cargar datos de forma asíncrona antes de ir al juego
            CargarDatosSesion(user.UserId);
        }
        else if (user.IsAnonymous)
        {
            irAlJuego = true; // Si es invitado, va directo
        }
        else
        {
            recargarEscena = true;
        }"""

# A veces los acentos se corrompen en latin-1, vamos a usar un target que esquiva la palabra "asíncrona" si falla.
target_lines = [
    "FirebaseUser user = auth.CurrentUser;",
    "if (user.IsEmailVerified)",
    "{",
    "// Cargar datos de forma",
    "CargarDatosSesion(user.UserId);",
    "}",
    "else if (user.IsAnonymous)",
    "{",
    "irAlJuego = true;",
    "}",
    "else",
    "{",
    "recargarEscena = true;",
    "}"
]

replacement = """        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                // Cargar datos de forma asíncrona antes de ir al juego
                CargarDatosSesion(user.UserId);
            }
            else if (user.IsAnonymous)
            {
                irAlJuego = true; // Si es invitado, va directo
            }
            else
            {
                recargarEscena = true;
            }
        }"""

# Manual replacement line by line to avoid exact whitespace issues
import re
new_content = re.sub(
    r"FirebaseUser user = auth\.CurrentUser;\s+if \(user\.IsEmailVerified\)\s+\{[\s\S]+?recargarEscena = true;\s+\}",
    replacement,
    content
)

with open(file_path, 'w', encoding='latin-1') as f:
    f.write(new_content)

print("Patch applied to AuthManager.cs")
