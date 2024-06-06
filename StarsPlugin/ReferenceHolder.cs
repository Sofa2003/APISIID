using TFlex;
using TFlex.Model;
using TFlex.Model.Model2D;

namespace Stars
{
	/// <summary>
	/// T - ссылочный объект класса Node или Variable, который мы оборачиваем
	/// и синхронизируем по коду типа int с владельцем объекта документа,
	/// который имеет тип TFlex.Model.Model2D.ExternalObject
	/// </summary>
	public class ReferenceHolder<T>
		where T : ModelObject
	{
		//содержимое Holder-а для хранения объекта в режиме modeKeepingObject (автономном от owner-а), изначально пусто
		private T _object;

		//ссылка на owner-а звезды, которой принадлежит данный Holder, нужна для режима modeKeepingReference (Holder подключен к owner-у)
		private StarObject _star;
		
		//код для связи с Owner-ом (владеющим данной звездой External Object-ом)
		private int _code;

		public ReferenceHolder (int code, StarObject star)
		{
			_code = code;
			//теперь мы связаны с owner-ом
			_star = star;
			//внутренний объект нужен нам только в случае, если owner будет пустым
			_object = null;
		}

		/// <summary>
		/// Хранит обёрнутый объект либо автономно по ссылке, либо используя механизм ссылок родительского объекта
		/// </summary>
		public T Value
		{
			get
			{
				if (_star.Owner2D != null)
					return _star.Owner2D.GetReference(_code) as T;

				return _object;
			}

			set
			{
				_object = value;				
			}
		
		}

		/// <summary>
		/// устанавливает Holder-а, возвращает true, если успешно
		/// </summary>
		public void SetReference(T source)
		{ 
			if (_star.Owner2D != null)
			{
				_star.Owner2D.SetReference(_code, source);
			}
            else
                _object = source;		
		}

		/// <summary>
		/// сбрасывает Holder-а (т.к. используется лишь с автономными звёздами, Owner2D можно не сбрасывать - он null)
		/// </summary>
		public void ReleaseReference() 
		{ 			
			_object = null; 
		}

	}//class ends

}//namespace ends