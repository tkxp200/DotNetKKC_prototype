# DotNetKKC_prototype

## 概要

C#でかな漢字変換を実現するライブラリです。以下の特徴を備えています。

- Mozc辞書ベース
- [AhoCorasickDoubleArrayTrie](https://github.com/nreco/AhoCorasickDoubleArrayTrie?tab=Apache-2.0-1-ov-file)を使用した高速な検索を採用
- NBest変換取得のみのシンプルな機能

このライブラリは開発中であり、別のリポジトリでさらに開発を進めていく予定です。そのため現在はパッケージでの配布予定はありません。

## 使い方

使用には.NET SDKが必要です。

+ リポジトリのクローン
  
  ```
  git clone https://github.com/tkxp200/DotNetKKC_prototype.git
  ```
+ 新しいConsoleプロジェクトを作成

  ```
  dotnet new console -o "DotNetKKCTest"
  cd DotNetKKCTest
  ```
+ DotNetKKCを依存関係に追加
  
  ```
  dotnet add reference ../DotNetKKC_prototype/DotNetKKC.csproj
  ```
+ 辞書データの配置
  
  dicディレクトリを作成し[Google Mozcのsrc/data/dictionary_oss](https://github.com/google/mozc/tree/master/src/data/dictionary_oss)配下にある`dictionary*.txt`および`connection_single_column.txt`を配置します。

  検証はしていませんが[mozc-ut辞書](https://github.com/utuhiro78/merge-ut-dictionaries/tree/main)も使用できると思います。
  ```
  mkdir dic
  ```
  dicディレクトリ配下に次のファイルがあればOKです。
  ```
  +---dic
  |       connection_single_column.txt
  |       dictionary00.txt
  |       dictionary01.txt
  |       dictionary02.txt
  |       dictionary03.txt
  |       dictionary04.txt
  |       dictionary05.txt
  |       dictionary06.txt
  |       dictionary07.txt
  |       dictionary08.txt
  |       dictionary09.txt
  ```
+ DoubleArrayの生成
  
  `Program.cs`を次のように編集し変換に必要なDoubleArrayファイルをdicフォルダ配下にあるtxtファイルを用いて生成します。
  - Program.cs

    ```cs
    using DotNetKKC;

    string filePath = @"dic/dictionary.doublearray";

    var generator = new DoubleArrayGenerator(filePath);

    ```
  - 実行
 
    ```bash
    dotnet build
    dotnet run
    ```
    実行には30秒～2分ほどかかります。
  
    dicディレクトリ配下に`dictionary.doublearray`が生成されていればOKです。
    ```
    $ ls dic
    connection_single_column.txt dictionary04.txt
    dictionary.doublearray       dictionary05.txt
    dictionary00.txt             dictionary06.txt
    dictionary01.txt             dictionary07.txt
    dictionary02.txt             dictionary08.txt
    dictionary03.txt             dictionary09.txt
    ```
+ かな漢字変換

  `GetConversion`関数を呼び出すと変換後の文字列とコストの辞書が返ってきます。\
  クラスのインスタンス化時に辞書を読みだすため3~5秒ほど時間がかかります。

  - Program.cs

    ```cs
    ﻿using DotNetKKC;

    string filePath = @"dic/dictionary.doublearray";
    
    var conversion = new Conversion(filePath);
    
    Dictionary<string, double> result = conversion.GetConversion("ここではきものをぬぐ", 20);
    
    int i = 0;
    foreach (KeyValuePair<string, double> item in result)
    {
        Console.WriteLine($"{i + 1:00}Best Conversion: {item.Value:000000}, {item.Key}");
        i++;
    }
    ```
    `GetConversion()`の第二引数で取得する変換候補の数nを設定できます。省略するとn=10の変換候補が返ってきます。
  - 実行
 
    ```
    $ dotnet run
    Convert: ここではきものをぬぐ
    01Best Conversion: 017550, ここでは着物を脱ぐ
    02Best Conversion: 018373, ここで履物を脱ぐ
    03Best Conversion: 018502, ここではきものを脱ぐ
    04Best Conversion: 019001, ここでは着物をぬぐ
    05Best Conversion: 019017, ここで履き物を脱ぐ
    06Best Conversion: 019824, ここで履物をぬぐ
    07Best Conversion: 019929, 個々では着物を脱ぐ
    08Best Conversion: 019953, ここではきものをぬぐ
    09Best Conversion: 020069, ここで破棄者を脱ぐ
    10Best Conversion: 020129, ここではキモノを脱ぐ
    11Best Conversion: 020443, ここで履きものを脱ぐ
    12Best Conversion: 020468, ここで履き物をぬぐ
    13Best Conversion: 020552, ここではき物を脱ぐ
    14Best Conversion: 020747, 此処では着物を脱ぐ
    15Best Conversion: 020752, 個々で履物を脱ぐ
    16Best Conversion: 020881, 個々ではきものを脱ぐ
    17Best Conversion: 021021, ここでは気物を脱ぐ
    18Best Conversion: 021025, ここで吐きものを脱ぐ
    19Best Conversion: 021065, ココでは着物を脱ぐ
    20Best Conversion: 021129, ここで破棄物を脱ぐ
    ```
    
