﻿namespace Kata.Wallet.Domain;

public class Wallet
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public string UserDocument { get; set; }
    public string UserName { get; set; } 
    public Currency Currency { get; set; }
    public List<Transaction>? IncomingTransactions { get; set; } = new List<Transaction>();
    public List<Transaction>? OutgoingTransactions { get; set; } = new List<Transaction>();
}

public enum Currency
{
    USD,
    EUR,
    ARS
}
