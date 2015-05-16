﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CollectiveIntelligence.Core
{
    public static class Similarity
    {
        public static double GetEuclideanDistance(Preferences preferences, Person person1, Person person2)
        {
            var person1Prefs = preferences.GetPreferencesByPersonId(person1.Id);
            var person2Prefs = preferences.GetPreferencesByPersonId(person2.Id);

            var sharedItems = person1Prefs.Where(currentPref => person2Prefs.Any(pref => pref._entity == currentPref._entity))
                .ToDictionary(currentPref => currentPref._entity, currentPref => true);

            if (!sharedItems.Any())
            {
                return 0;
            }

            var sumOfSquares = sharedItems.Select(
                si =>
                    Math.Pow(person1Prefs.First(pref => pref._entity == si.Key)._score -
                             person2Prefs.First(pref => pref._entity == si.Key)._score, 2)).Sum();
            
            return 1/(1 + Math.Sqrt(sumOfSquares));

        }

        public class Preferences : IEnumerable<Preference>
        {
            private List<Preference> _preferences;

            public Preferences(List<Preference> preferences)
            {
                _preferences = preferences;
            }

            public IEnumerable<Preference> GetPreferencesByPersonId(PersonId id)
            {
                if (id == null) throw new ArgumentNullException("id");
                return _preferences.Where(pref => pref._person.Id == id);
            }

            public IEnumerator<Preference> GetEnumerator()
            {
                return _preferences.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _preferences.GetEnumerator();
            }
        }

        public class PersonId
        {
            private readonly Guid _value;

            public PersonId()
            {
                _value = Guid.NewGuid();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                var pId = (PersonId)obj;
                return _value == pId._value;
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            public override string ToString()
            {
                return _value.ToString();
            }

            public static bool operator ==(PersonId id1, PersonId id2)
            {
                return !ReferenceEquals(id1, null) && id1.Equals(id2);
            }

            public static bool operator !=(PersonId id1, PersonId id2)
            {
                return !(id1 == id2);
            }
        }

        public class Person
        {
            public readonly PersonId Id;

            public Person()
            {
                Id = new PersonId();
            }
            public string Name { get; set; }
        }

        public class Preference
        {
            public Person _person;
            public object _entity;
            public double _score;

            public Preference(Person person, object entity, double score)
            {
                _score = score;
                _entity = entity;
                _person = person;
            }
        }
    }

    public static class Similarity<TEntity, TItem>
    {
        public static double GetEuclideanDistance(Dictionary<TEntity, Dictionary<TItem, double>> preferences,
    TEntity entity1, TEntity entity2)
        {
            if (preferences == null)
            {
                throw new ArgumentNullException("preferences");
            }

            if (entity1 == null)
            {
                throw new ArgumentNullException("entity1");
            }

            if (entity2 == null)
            {
                throw new ArgumentNullException("entity2");
            }

            if (!preferences.ContainsKey(entity1) || !preferences.ContainsKey(entity2))
            {
                return 0;
            }

            var similarities = preferences[entity1].Where(pref => preferences[entity2].ContainsKey(pref.Key)).Select(pair => pair.Key).ToArray();
            if (!similarities.Any())
            {
                return 0;
            }

            var sumOfSquares = similarities.Select(sim => Math.Pow(preferences[entity1][sim] - preferences[entity2][sim], 2)).Sum();
            return 1/(1 + Math.Sqrt(sumOfSquares));
        }

        public static double GetPearsonCorrelation(Dictionary<TEntity, Dictionary<TItem, double>> preferences, TEntity entity1, TEntity entity2)
        {
            if (preferences == null)
            {
                throw new ArgumentNullException("preferences");
            }

            if (entity1 == null)
            {
                throw new ArgumentNullException("entity1");
            }

            if (entity2 == null)
            {
                throw new ArgumentNullException("entity2");
            }

            if (!preferences.ContainsKey(entity1) || !preferences.ContainsKey(entity2))
            {
                return 0;
            }

            var similarities = preferences[entity1].Where(pref => preferences[entity2].ContainsKey(pref.Key)).Select(pair => pair.Key).ToArray();
            if (!similarities.Any())
            {
                return 0;
            }
            var similaritiesCount = similarities.Count();

            // Add up all the preferences
            //  sum1=sum([prefs[p1][it] for it in si])
            //  sum2=sum([prefs[p2][it] for it in si])

            var sum1 = similarities.Select(sim => preferences[entity1][sim]).Sum();
            var sum2 = similarities.Select(sim => preferences[entity2][sim]).Sum();


            //  # Sum up the squares
            //  sum1Sq=sum([pow(prefs[p1][it],2) for it in si])
            //  sum2Sq=sum([pow(prefs[p2][it],2) for it in si])

            var sumOfSquares1 = similarities.Select(sim => Math.Pow(preferences[entity1][sim], 2)).Sum();
            var sumOfSquares2 = similarities.Select(sim => Math.Pow(preferences[entity2][sim], 2)).Sum();


            //  # Sum up the products
            //  pSum=sum([prefs[p1][it]*prefs[p2][it] for it in si])

            var sumOfProducts = similarities.Select(sim => preferences[entity1][sim]*preferences[entity2][sim]).Sum();

            //  # Calculate Pearson scorenum=pSum−(sum1*sum2/n)
            //  den=sqrt((sum1Sq−pow(sum1,2)/n)*(sum2Sq−pow(sum2,2)/n))
            //  if den==0: return 0
            //  r=num/den”

            var numerator = sumOfProducts - (sum1*sum2)/similaritiesCount;
            var denominator =
                Math.Sqrt((sumOfSquares1 - Math.Pow(sum1, 2)/similaritiesCount)*
                          (sumOfSquares2 - Math.Pow(sum2, 2)/similaritiesCount));

            if (denominator == 0)
            {
                return 0;
            }

            return numerator/denominator;

            //Excerpt From: Toby Segaran. “Programming Collective Intelligence.” iBooks. 
        }
    }
}
