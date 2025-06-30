# possumus

Kata.Wallet.Api es una API RESTful desarrollada en .NET 6, diseñada para administrar wallets (billeteras virtuales) y transactions (transacciones) entre ellas.

Permite crear wallets, consultar wallets existentes y realizar transferencias validadas entre billeteras.

⚙️ Endpoints
🔹 Wallet
1. GET /api/Wallet
Descripción: Obtiene todas las wallets existentes.

Parámetros opcionales:

document: para filtrar wallets asociadas a un documento de usuario.

currency: para filtrar wallets por tipo de moneda.

Respuesta: Lista de wallets que cumplen con los filtros (o todas, si no se pasan filtros).

2. GET /api/Wallet/{id}
Descripción: Obtiene una wallet específica por su id.

Respuesta: Wallet correspondiente, o error si no existe.

3. POST /api/Wallet
Descripción: Crea una nueva wallet.

Cuerpo: Datos de la wallet a crear (ej. usuario, documento, moneda inicial, balance inicial, etc.).

Respuesta: Wallet creada.
Cuerpo:
{
  "id": 5,
  "balance": 100,
  "userDocument": "11111111",
  "userName": "Jose Lopez",
  "currency": "USD"
}

🔹 Transaction
1. GET /api/Transaction
Descripción: Obtiene todas las transacciones asociadas a una wallet específica.

Parámetro requerido:

walletId: identifica la wallet para traer sus transacciones.

Respuesta: Lista de transacciones relacionadas con la wallet indicada.

2. POST /api/Transaction
Descripción: Crea una nueva transacción entre dos wallets.
Importante: NO enviar Date ni Id, estos son campos que se setean automáticamente. Leer puntos de mejora.
Cuerpo:
{
  "amount": 10,
  "description": "Money loan",
  "originWalletId": 1,
  "destinationWalletId": 2
}

Validaciones:

La wallet origen debe existir.

La wallet destino debe existir.

Ambas wallets deben tener la misma moneda.

No se permite transferir a la misma wallet.

La wallet origen debe tener saldo suficiente.

Proceso:

Si todo es válido, se crea la transacción.

Se actualizan los balances de ambas wallets.

Se devuelve la transacción creada.

Posibles errores:

WalletDoesNotExistException: alguna wallet no existe.

WalletsCurrenciesDoNotMatchException: monedas no coinciden.

TransactionMustBeBetweenDifferentAccountsException: wallets son iguales.

InsufficientBalanceException: saldo insuficiente.

# Puntos de mejora

1. Decidí crear solo algunos tests de integración y unitarios, los mas útiles, pero se podrían agregar más.
2. Se podría agregar autenticación y autorización (ej: JWT)
3. Se podría crear un DTO para la request y para la response por separado, para wallets y para transactions. No lo hice porque es un proyecto chico y fácil de manejar, pero esto es útil para que, por ejemplo, los campos Date e Id de transaction no se envíen en el POST sino que se seteen manualmente.
4. El id de la wallet se envía como parámetro en la request. Esta no es la mejor opción, yo generaría un Guid por cada nueva wallet creada, pero de cualquier manera las wallets suelen tener un número de cuenta que se setea manualmente de cualquier manera, por lo que opté por mantener el id en la request con una validación.
5. No suelo devolver los objetos recién creados en los endpoints POST, pero en este caso los devuelvo para poder corroborarlos en los tests.
