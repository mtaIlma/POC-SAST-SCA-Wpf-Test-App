## Vulnerable Wpf test application.

Used to test SAST/SCA tools.

It contains at least the following vulnerabilities:

##### Components

- Azure identity 1.10.4
- NHibernate 5.4.6
- Npgsql 7.0.6

##### Code

`XmlSigningService`:

- Xml injection.

`KeyVaultService`:

- Private key stored as string.
  

`UserService`:

- Sql injection.

`XamlGeneratorWindow.xaml.cs`

- Xml injection.

`NHibernateHelper.cs`

- Database credentials in the source code. 