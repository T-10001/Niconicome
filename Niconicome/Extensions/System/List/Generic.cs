﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niconicome.Extensions.System.List
{
    static class List
    {
        /// <summary>
        /// リストにユニークデータを追加する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<T> AddUnique<T>(this List<T> list, T data)
        {
            if (!list.Contains(data))
            {
                list.Add(data);
            }
            return list;
        }

        /// <summary>
        /// 指定した条件をクリアした場合にリストにユニークデータを挿入する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T> AddUnique<T>(this List<T> list, T data, Predicate<List<T>> predicate)
        {
            if (predicate(list))
            {
                list.Add(data);
            }
            return list;
        }

        /// <summary>
        /// 選択関数を与えて同一要素を削除する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TMem"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, TMem>(this IEnumerable<T> source, Func<T, TMem> selector)
        {
            var comparer = new EqualityComparer<T, TMem>(selector);
            return source.Distinct(comparer);
        }

        /// <summary>
        /// リストをコピーする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Copy<T>(this IEnumerable<T> source)
        {
            return new List<T>(source);
        }

        /// <summary>
        /// WhenAllを後付けで実行する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> source)
        {
            return (await Task.WhenAll(source)).AsEnumerable();
        }
    }

    public class EqualityComparer<T, TMem> : IEqualityComparer<T>
    {
        private readonly Func<T, TMem> selector;

        public EqualityComparer(Func<T, TMem> selector)
        {
            this.selector = selector;
        }

        public bool Equals(T? a, T? b)
        {
            if (a is null && b is null) return true;
            if (a is null ^ b is null) return false;
            return this.selector(a!)!.Equals(this.selector(b!));
        }

        public int GetHashCode(T obj)
        {
            if (obj is null) return 0;
            return this.selector(obj)?.GetHashCode() ?? 0;
        }
    }
}
